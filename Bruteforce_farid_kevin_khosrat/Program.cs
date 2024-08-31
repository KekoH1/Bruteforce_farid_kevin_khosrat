

using System;
using System.Threading;
using OtpNet;

namespace BruteForceWith2FAExample
{
    internal class Program
    {
        private static int failedLoginAttempts = 0;
        private static int maxFailedAttempts = 3;
        private static bool isBlocked = false;
        private static DateTime blockUntil = DateTime.MinValue;

        static void Main(string[] args)
        {
            Console.Write("Password (max 6 letters, recommended 4): ");
            String password = Console.ReadLine();

            String current = "Farid";
            int[] pos = { 0, 0, 0, 0, 0, 0 };

            // Expanded alphabet including uppercase, digits, and special characters
            String[] alphabet = { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                                  "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                                  "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                                  "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "=", "+", "[", "]", "{", "}", ";", ":", "'", "\"", ",", ".", "/", "?", "<", ">", "|"
            };

            int count = 0;
            int maxAttempts = 1000000; // Limit the maximum number of attempts to prevent infinite loops
            int attemptInterval = 0; // Delay in milliseconds between each attempt

            // Function to convert pos array to string
            string ConvertPosToString(int[] positions, string[] alpha)
            {
                char[] chars = new char[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    chars[i] = positions[i] == 0 ? '\0' : alpha[positions[i]][0];
                }
                return new string(chars);
            }

            while (!current.Equals(password) || !Perform2FA())
            {
                if (isBlocked && DateTime.Now < blockUntil)
                {
                    Console.WriteLine("Your account is temporarily blocked due to multiple failed login attempts. Please try again later.");
                    Thread.Sleep(5000);
                    continue;
                }

                if (count >= maxAttempts)
                {
                    Console.WriteLine("Maximum attempts reached. Terminating brute force attack.");
                    return;
                }

                // Increment positions and generate the current string
                for (int i = 0; i < pos.Length; i++)
                {
                    if (pos[i] < alphabet.Length - 1)
                    {
                        pos[i]++;
                        break;
                    }
                    else
                    {
                        pos[i] = 0;
                    }
                }

                current = ConvertPosToString(pos, alphabet);

                if (count % 100 == 0) Console.WriteLine(current);
                count++;

                if (current == password)
                {
                    Console.WriteLine("Password is correct. Proceeding to 2FA...");
                    // Attempt 2FA; if it fails, we increase the failed login attempts
                    if (!Perform2FA())
                    {
                        failedLoginAttempts++;
                        if (failedLoginAttempts >= maxFailedAttempts)
                        {
                            Console.WriteLine("Too many failed attempts. Blocking account for 5 minutes.");
                            isBlocked = true;
                            blockUntil = DateTime.Now.AddMinutes(5);
                            failedLoginAttempts = 0;
                        }
                    }
                }
                else
                {
                    failedLoginAttempts++;
                    Console.WriteLine("Incorrect password.");
                }

                if (failedLoginAttempts >= maxFailedAttempts)
                {
                    Console.WriteLine("Too many failed attempts. Blocking account for 5 minutes.");
                    isBlocked = true;
                    blockUntil = DateTime.Now.AddMinutes(5);
                    failedLoginAttempts = 0;
                }

                Thread.Sleep(attemptInterval); // Delay between attempts
            }

            Console.WriteLine($"Found password: {current}");
        }

        // Method to handle 2FA logic
        static bool Perform2FA()
        {
            string userSecretKey = "JBSWY3DPEHPK3PXP"; // Example key, should be user-specific
            var otp = new Totp(Base32Encoding.ToBytes(userSecretKey));
            string twoFactorCode = otp.ComputeTotp();

            Console.WriteLine($"Your 2FA code is: {twoFactorCode}");

            Console.Write("Enter 2FA code: ");
            string userCode = Console.ReadLine();

            bool isCodeValid = otp.VerifyTotp(userCode, out long timeStepMatched, new VerificationWindow(previous: 2, future: 2));

            if (isCodeValid)
            {
                Console.WriteLine("2FA authentication successful!");
                return true;
            }
            else
            {
                Console.WriteLine("Invalid 2FA code.");
                return false;
            }
        }
    }
}

