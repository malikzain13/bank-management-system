using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ZainNationalBank
{
    class Transaction
    {
        public string Type { get; set; }
        public double Amount { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; }
    }

    class Loan
    {
        public double TotalAmount { get; set; }
        public double RemainingAmount { get; set; }
        public int Months { get; set; }
    }

    class Account
    {
        public int AccountNumber { get; set; }
        public string Name { get; set; }
        public string CNIC { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AccountType { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }

        public Loan Loan { get; set; } = null;
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public void Deposit(double amount, string details = "Deposit")
        {
            Balance += amount;
            Transactions.Add(new Transaction
            {
                Type = "Deposit",
                Amount = amount,
                Details = details,
                Date = DateTime.Now
            });
            Console.WriteLine("Deposit Successful!");
        }

        public void Withdraw(double amount)
        {
            if (amount <= Balance)
            {
                Balance -= amount;
                Transactions.Add(new Transaction
                {
                    Type = "Withdraw",
                    Amount = amount,
                    Details = "Cash Withdraw",
                    Date = DateTime.Now
                });
                Console.WriteLine("Withdraw Successful!");
            }
            else Console.WriteLine("Insufficient Balance!");
        }

        public void ApplyInterest()
        {
            if (AccountType.ToLower() == "saving")
            {
                double interest = Balance * 0.05;
                Balance += interest;

                Transactions.Add(new Transaction
                {
                    Type = "Interest",
                    Amount = interest,
                    Details = "Yearly Interest",
                    Date = DateTime.Now
                });

                Console.WriteLine($"Interest Added: {interest}");
            }
            else Console.WriteLine("Only Saving Accounts get interest!");
        }

        public void TakeLoan(double amount, int months)
        {
            if (Loan != null)
            {
                Console.WriteLine("Loan already exists!");
                return;
            }

            double total = amount * 1.1;

            Loan = new Loan
            {
                TotalAmount = total,
                RemainingAmount = total,
                Months = months
            };

            Balance += amount;

            Transactions.Add(new Transaction
            {
                Type = "Loan Taken",
                Amount = amount,
                Details = $"Loan {months} months",
                Date = DateTime.Now
            });

            Console.WriteLine($"Loan Approved! Total Payable: {total}");
        }

        public void PayLoan(double amount)
        {
            if (Loan == null)
            {
                Console.WriteLine("No Loan Found!");
                return;
            }

            if (amount > Balance)
            {
                Console.WriteLine("Insufficient Balance!");
                return;
            }

            Balance -= amount;
            Loan.RemainingAmount -= amount;

            Transactions.Add(new Transaction
            {
                Type = "Loan Payment",
                Amount = amount,
                Details = "Loan Repayment",
                Date = DateTime.Now
            });

            if (Loan.RemainingAmount <= 0)
            {
                Console.WriteLine("Loan Cleared!");
                Loan = null;
            }
        }

        public void ShowDetails()
        {
            Console.WriteLine($"\nAcc#: {AccountNumber}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Balance: {Balance}");

            if (Loan != null)
                Console.WriteLine($"Loan Remaining: {Loan.RemainingAmount}");
        }

        public void ShowTransactions()
        {
            Console.WriteLine("\n--- Transactions ---");
            foreach (var t in Transactions)
                Console.WriteLine($"{t.Date} | {t.Type} | {t.Amount} | {t.Details}");
        }
    }

    class Program
    {
        static List<Account> accounts = new List<Account>();
        static string filePath = "bankdata.json";

        static void Main()
        {
            LoadData();

            int choice;
            do
            {
                ShowHeader();

                ShowMenu(new string[]
                {
                    "1. Create Account",
                    "2. Login",
                    "3. Admin Panel",
                    "4. Exit"
                });

                choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1: CreateAccount(); break;
                    case 2: Login(); break;
                    case 3: AdminPanel(); break;
                }

            } while (choice != 4);

            SaveData();
        }

        // 🔥 STEP-BY-STEP MENU FUNCTION
        static void ShowMenu(string[] options)
        {
            Console.WriteLine("\nChoose Option:\n");
            foreach (var opt in options)
            {
                Console.WriteLine(opt);
                Thread.Sleep(200); // delay
            }
        }

        static void ShowHeader()
        {
            Console.Clear();
            string bank = "ZAIN NATIONAL BANK";
            int left = (Console.WindowWidth - bank.Length) / 2;
            Console.SetCursorPosition(left, 1);
            Console.WriteLine(bank);
            Console.WriteLine("\n=====================================\n");
        }

        static void CreateAccount()
        {
            Console.Write("Account No: ");
            int accNo = Convert.ToInt32(Console.ReadLine());

            Console.Write("Name: ");
            string name = Console.ReadLine();

            Console.Write("CNIC: ");
            string cnic = Console.ReadLine();

            Console.Write("Phone: ");
            string phone = Console.ReadLine();

            Console.Write("Address: ");
            string address = Console.ReadLine();

            Console.Write("Type (Saving/Current): ");
            string type = Console.ReadLine();

            Console.Write("Password: ");
            string pass = ReadPassword();

            Console.Write("Initial Deposit: ");
            double amount = Convert.ToDouble(Console.ReadLine());

            Account acc = new Account
            {
                AccountNumber = accNo,
                Name = name,
                CNIC = cnic,
                Phone = phone,
                Address = address,
                AccountType = type,
                Password = pass
            };

            if (amount > 0)
                acc.Deposit(amount, "Initial Deposit");

            accounts.Add(acc);
            Console.WriteLine("Account Created!");
        }

        static void Login()
        {
            Console.Write("Account No: ");
            int accNo = Convert.ToInt32(Console.ReadLine());

            Console.Write("Password: ");
            string pass = ReadPassword();

            var acc = accounts.Find(a => a.AccountNumber == accNo && a.Password == pass);

            if (acc != null)
                AccountMenu(acc);
            else
                Console.WriteLine("Invalid Login!");
        }

        static void AccountMenu(Account acc)
        {
            int choice;
            do
            {
                ShowMenu(new string[]
                {
                    "1. Deposit",
                    "2. Withdraw",
                    "3. Transfer",
                    "4. Details",
                    "5. Transactions",
                    "6. ATM",
                    "7. Interest",
                    "8. Take Loan",
                    "9. Pay Loan",
                    "10. Logout"
                });

                choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        Console.Write("Amount: ");
                        acc.Deposit(Convert.ToDouble(Console.ReadLine()));
                        break;
                    case 2:
                        Console.Write("Amount: ");
                        acc.Withdraw(Convert.ToDouble(Console.ReadLine()));
                        break;
                    case 3:
                        Transfer(acc);
                        break;
                    case 4:
                        acc.ShowDetails();
                        break;
                    case 5:
                        acc.ShowTransactions();
                        break;
                    case 6:
                        ATM(acc);
                        break;
                    case 7:
                        acc.ApplyInterest();
                        break;
                    case 8:
                        Console.Write("Loan Amount: ");
                        double la = Convert.ToDouble(Console.ReadLine());
                        Console.Write("Months: ");
                        int m = Convert.ToInt32(Console.ReadLine());
                        acc.TakeLoan(la, m);
                        break;
                    case 9:
                        Console.Write("Pay Amount: ");
                        acc.PayLoan(Convert.ToDouble(Console.ReadLine()));
                        break;
                }

            } while (choice != 10);
        }

        static void ATM(Account acc)
        {
            int c;
            do
            {
                ShowMenu(new string[]
                {
                    "1. Fast Cash (1000)",
                    "2. Balance",
                    "3. Statement",
                    "4. Exit"
                });

                c = Convert.ToInt32(Console.ReadLine());

                if (c == 1) acc.Withdraw(1000);
                else if (c == 2) Console.WriteLine($"Balance: {acc.Balance}");
                else if (c == 3) acc.ShowTransactions();

            } while (c != 4);
        }

        static void Transfer(Account sender)
        {
            Console.Write("Receiver Acc: ");
            int rec = Convert.ToInt32(Console.ReadLine());

            var r = accounts.Find(a => a.AccountNumber == rec);

            if (r == null)
            {
                Console.WriteLine("Receiver not found!");
                return;
            }

            Console.Write("Amount: ");
            double amt = Convert.ToDouble(Console.ReadLine());

            if (amt <= sender.Balance)
            {
                sender.Balance -= amt;
                r.Balance += amt;

                sender.Transactions.Add(new Transaction
                {
                    Type = "Transfer Sent",
                    Amount = amt,
                    Details = $"To {rec}",
                    Date = DateTime.Now
                });

                r.Transactions.Add(new Transaction
                {
                    Type = "Transfer Received",
                    Amount = amt,
                    Details = $"From {sender.AccountNumber}",
                    Date = DateTime.Now
                });

                Console.WriteLine("Transfer Successful!");
            }
            else Console.WriteLine("Insufficient Balance!");
        }

        static void AdminPanel()
        {
            Console.Write("Admin Password: ");
            if (Console.ReadLine() != "admin123")
            {
                Console.WriteLine("Wrong Password!");
                return;
            }

            int ch;
            do
            {
                ShowMenu(new string[]
                {
                    "1. View All",
                    "2. View Details",
                    "3. Transactions",
                    "4. Delete",
                    "5. Search",
                    "6. Back"
                });

                ch = Convert.ToInt32(Console.ReadLine());

                Account acc = FindAccount();

                switch (ch)
                {
                    case 1:
                        foreach (var a in accounts)
                            Console.WriteLine($"{a.AccountNumber} | {a.Name} | {a.Balance}");
                        break;
                    case 2:
                        acc?.ShowDetails();
                        break;
                    case 3:
                        acc?.ShowTransactions();
                        break;
                    case 4:
                        if (acc != null)
                        {
                            accounts.Remove(acc);
                            Console.WriteLine("Deleted!");
                        }
                        break;
                    case 5:
                        acc?.ShowDetails();
                        break;
                }

            } while (ch != 6);
        }

        static Account FindAccount()
        {
            Console.Write("Enter Account No: ");
            int no = Convert.ToInt32(Console.ReadLine());
            return accounts.Find(a => a.AccountNumber == no);
        }

        static string ReadPassword()
        {
            string p = "";
            ConsoleKeyInfo k;

            do
            {
                k = Console.ReadKey(true);
                if (k.Key != ConsoleKey.Enter)
                {
                    p += k.KeyChar;
                    Console.Write("*");
                }
            } while (k.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return p;
        }

        static void SaveData()
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(accounts));
        }

        static void LoadData()
        {
            if (File.Exists(filePath))
                accounts = JsonSerializer.Deserialize<List<Account>>(File.ReadAllText(filePath));
        }
    }
}
