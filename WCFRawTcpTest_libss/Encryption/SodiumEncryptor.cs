﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shadowsocks.Encryption
{
    public class SodiumEncryptor
        : IVEncryptor, IDisposable
    {
        const int CIPHER_SALSA20 = 1;
        const int CIPHER_CHACHA20 = 2;

        const int SODIUM_BLOCK_SIZE = 64;

        protected int _encryptBytesRemaining;
        protected int _decryptBytesRemaining;
        protected ulong _encryptIC;
        protected ulong _decryptIC;
        protected byte[] _encryptBuf;
        protected byte[] _decryptBuf;

        public SodiumEncryptor(string method, string password)
            : base(method, password)
        {
            InitKey(method, password);
            _encryptBuf = new byte[MAX_INPUT_SIZE + SODIUM_BLOCK_SIZE];
            _decryptBuf = new byte[MAX_INPUT_SIZE + SODIUM_BLOCK_SIZE];
        }

        private static Dictionary<string, int[]> _ciphers = new Dictionary<string, int[]> {
                {"salsa20", new int[]{32, 8, CIPHER_SALSA20, PolarSSL.AES_CTX_SIZE}},
                {"chacha20", new int[]{32, 8, CIPHER_CHACHA20, PolarSSL.AES_CTX_SIZE}},
        };

        protected override Dictionary<string, int[]> getCiphers()
        {
            return _ciphers;
        }

        public static List<string> SupportedCiphers()
        {
            return new List<string>(_ciphers.Keys);
        }

        protected override void cipherUpdate(bool isCipher, int length, byte[] buf, byte[] outbuf)
        {
            // TODO write a unidirection cipher so we don't have to if if if
            int bytesRemaining;
            ulong ic;
            byte[] sodiumBuf;
            byte[] iv;
            if (isCipher)
            {
                bytesRemaining = _encryptBytesRemaining;
                ic = _encryptIC;
                sodiumBuf = _encryptBuf;
                iv = _encryptIV;
            }
            else
            {
                bytesRemaining = _decryptBytesRemaining;
                ic = _decryptIC;
                sodiumBuf = _decryptBuf;
                iv = _decryptIV;
            }
            int padding = bytesRemaining;
            Buffer.BlockCopy(buf, 0, sodiumBuf, padding, length);

            switch (_cipher)
            {
                case CIPHER_SALSA20:
                    Sodium.crypto_stream_salsa20_xor_ic(sodiumBuf, sodiumBuf, (ulong)(padding + length), iv, ic, _key);
                    break;
                case CIPHER_CHACHA20:
                    Sodium.crypto_stream_chacha20_xor_ic(sodiumBuf, sodiumBuf, (ulong)(padding + length), iv, ic, _key);
                    break;
            }
            Buffer.BlockCopy(sodiumBuf, padding, outbuf, 0, length);
            padding += length;
            ic += (ulong)padding / SODIUM_BLOCK_SIZE;
            bytesRemaining = padding % SODIUM_BLOCK_SIZE;

            if (isCipher)
            {
                _encryptBytesRemaining = bytesRemaining;
                _encryptIC = ic;
            }
            else
            {
                _decryptBytesRemaining = bytesRemaining;
                _decryptIC = ic;
            }
        }

        public override void Dispose()
        {
        }
    }
}
