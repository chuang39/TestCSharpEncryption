using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace RNCryptor
{
public class Encryptor: Cryptor {
    private Schema defaultSchemaVersion = Schema.V2;

		public byte[] Encrypt (byte[] plainBytes, string password)
		{
			return this.Encrypt (plainBytes, password, this.defaultSchemaVersion);
		}

		public byte[] Encrypt (byte[] plainBytes, string password, Schema schemaVersion)
		{
			this.configureSettings (schemaVersion);
            

			PayloadComponents components = new PayloadComponents();
			components.schema = new byte[] {(byte)schemaVersion};
			components.options = new byte[] {(byte)this.options};
			components.salt = this.generateRandomBytes (Cryptor.saltLength);
			components.hmacSalt = this.generateRandomBytes (Cryptor.saltLength);
			components.iv = this.generateRandomBytes (Cryptor.ivLength);

			byte[] key = this.generateKey (components.salt, password);

			switch (this.aesMode) {
				case AesMode.CTR:
					components.ciphertext = this.encryptAesCtrLittleEndianNoPadding(plainBytes, key, components.iv);
					break;
					
				case AesMode.CBC:
					components.ciphertext = this.encryptAesCbcPkcs7(plainBytes, key, components.iv);
					break;
			}

			components.hmac = this.generateHmac(components, password);

			List<byte> binaryBytes = new List<byte>();
			binaryBytes.AddRange (this.assembleHeader(components));
			binaryBytes.AddRange (components.ciphertext);
			binaryBytes.AddRange (components.hmac);

            return binaryBytes.ToArray();
		}

		private byte[] encryptAesCbcPkcs7 (byte[] plaintext, byte[] key, byte[] iv)
		{
			var aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			var encryptor = aes.CreateEncryptor(key, iv);

			byte[] encrypted;

			using (var ms = new MemoryStream())
			{
				using (var cs1 = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
					cs1.Write(plaintext, 0, plaintext.Length);
				}

				encrypted = ms.ToArray ();
			}

			return encrypted;
		}
		
		private byte[] generateRandomBytes (int length)
		{
			byte[] randomBytes = new byte[length];
			var rng = new RNGCryptoServiceProvider ();
			rng.GetBytes (randomBytes);

			return randomBytes;
		}
      

  static void Main() {
    Console.WriteLine("Hello World!");
    Encryptor encryptor = new Encryptor();

    byte[] encryptedB64 = encryptor.Encrypt( Encoding.UTF8.GetBytes("hello"), "0x5057hudf88i42b57bad0000sssdsdsd65a28223dfdsfdfsadfsadf23211dfds2312");


    // Console.WriteLine(Encoding.ASCII.GetString( encryptedB64));
    foreach (var item in encryptedB64) {
        Console.WriteLine(item);
    }

  }
}
}
