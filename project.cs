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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Deposit Successful!");
            Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Withdraw Successful!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Insufficient Balance!");
            }
            Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Interest Added: {interest}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Only Saving Accounts get interest!");
            }
            Console.ResetColor();
        }

        public void TakeLoan(double amount, int months)
        {
            if (Loan != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Loan already exists!");
                Console.ResetColor();
                return;
            }

            double total = amount * 1.1;
            Loan = new Loan { TotalAmount = total, RemainingAmount = total, Months = months };
            Balance += amount;

            Transactions.Add(new Transaction { Type = "Loan Taken", Amount = amount, Details = $"Loan {months} months", Date = DateTime.Now });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loan Approved! Total Payable: {total}");
            Console.ResetColor();
        }

        public void PayLoan(double amount)
        {
            if (Loan == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Loan Found!");
                Console.ResetColor();
                return;
            }

            if (amount > Balance)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Insufficient Balance!");
                Console.ResetColor();
                return;
            }

            Balance -= amount;
            Loan.RemainingAmount -= amount;

            Transactions.Add(new Transaction { Type = "Loan Payment", Amount = amount, Details = "Loan Repayment", Date = DateTime.Now });

            Console.ForegroundColor = ConsoleColor.Green;
            if (Loan.RemainingAmount <= 0)
            {
                Console.WriteLine("Loan Cleared!");
                Loan = null;
            }
            else Console.WriteLine($"Repayment Successful! Remaining: {Loan.RemainingAmount}");
            Console.ResetColor();
        }

        public void ShowDetails()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nAcc#: {AccountNumber}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Balance: {Balance}");
            if (Loan != null) Console.WriteLine($"Loan Remaining: {Loan.RemainingAmount}");
            Console.ResetColor();
        }

        public void ShowTransactions()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- Transactions ---");
            Console.ResetColor();
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
                ShowMenu(new string[] {
                    "1. Create Account",
                    "2. Login",
                    "3. Admin Panel",
                    "4. Exit" });

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Enter Choice: ");
                string input = Console.ReadLine();
                int.TryParse(input, out choice);
                Console.ResetColor();

                switch (choice)
                {
                    case 1: CreateAccount(); break;
                    case 2: Login(); break;
                    case 3: AdminPanel(); break;
                }
                if (choice != 4) { Console.WriteLine("\nPress any key to continue..."); Console.ReadKey(); }

            } while (choice != 4);

            SaveData();
        }

        static void ShowMenu(string[] options)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n---------------------------------------------");
            Console.WriteLine("Choose Option:\n");
            Console.ResetColor();

            foreach (var opt in options)
            {
                Console.WriteLine(opt);
                Thread.Sleep(100);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("---------------------------------------------\n");
            Console.ResetColor();
        }

        static void ShowHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            string bank = "ZAIN NATIONAL BANK";
            int left = (Console.WindowWidth - bank.Length) / 2;
            Console.SetCursorPosition(left, 1);
            Console.WriteLine(bank);
            Console.WriteLine("\n" + new string('=', Console.WindowWidth));
            Console.ResetColor();
        }

        static void CreateAccount()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Account No: "); int accNo = Convert.ToInt32(Console.ReadLine());
            Console.Write("Name: "); string name = Console.ReadLine();
            Console.Write("CNIC: "); string cnic = Console.ReadLine();
            Console.Write("Phone: "); string phone = Console.ReadLine();
            Console.Write("Address: "); string address = Console.ReadLine();
            Console.Write("Type (Saving/Current): "); string type = Console.ReadLine();
            Console.Write("Password: "); string pass = ReadPassword();
            Console.Write("Initial Deposit: "); double amount = Convert.ToDouble(Console.ReadLine());
            Console.ResetColor();

            Account acc = new Account { AccountNumber = accNo, Name = name, CNIC = cnic, Phone = phone, Address = address, AccountType = type, Password = pass };
            if (amount > 0) acc.Deposit(amount, "Initial Deposit");

            accounts.Add(acc);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Account Created Successfully!");
            Console.ResetColor();
        }

        static void Login()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Account No: "); int accNo = Convert.ToInt32(Console.ReadLine());
            Console.Write("Password: "); string pass = ReadPassword();
            Console.ResetColor();

            var acc = accounts.Find(a => a.AccountNumber == accNo && a.Password == pass);
            if (acc != null) AccountMenu(acc);
            else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Invalid Login!"); Console.ResetColor(); }
        }

        static void AccountMenu(Account acc)
        {
            int choice;
            do
            {
                ShowHeader();
                Console.WriteLine($"Welcome, {acc.Name}");
                ShowMenu(new string[] {
                    "1. Deposit",
                    "2. Withdraw",
                    "3. Transfer",
                    "4. Details",
                    "5. Transactions",
                    "6. ATM",
                    "7. Interest",
                    "8. Take Loan",
                    "9. Pay Loan",
                    "10. Logout" });

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Action: "); choice = Convert.ToInt32(Console.ReadLine());
                Console.ResetColor();

                switch (choice)
                {
                    case 1: Console.Write("Amount: "); acc.Deposit(Convert.ToDouble(Console.ReadLine())); break;
                    case 2: Console.Write("Amount: "); acc.Withdraw(Convert.ToDouble(Console.ReadLine())); break;
                    case 3: Transfer(acc); break;
                    case 4: acc.ShowDetails(); break;
                    case 5: acc.ShowTransactions(); break;
                    case 6: ATM(acc); break;
                    case 7: acc.ApplyInterest(); break;
                    case 8:
                        Console.Write("Loan Amount: "); double la = Convert.ToDouble(Console.ReadLine());
                        Console.Write("Months: "); int m = Convert.ToInt32(Console.ReadLine());
                        acc.TakeLoan(la, m); break;
                    case 9: Console.Write("Pay Amount: "); acc.PayLoan(Convert.ToDouble(Console.ReadLine())); break;
                }
                if (choice != 10) { Console.WriteLine("\nPress any key..."); Console.ReadKey(); }
            } while (choice != 10);
        }

        static void ATM(Account acc)
        {
            int c;
            do
            {
                ShowHeader();
                Console.WriteLine("ATM MODE");
                ShowMenu(new string[] {
                    "1. Fast Cash (1000)",
                    "2. Balance",
                    "3. Statement",
                    "4. Exit" });
                c = Convert.ToInt32(Console.ReadLine());
                if (c == 1) acc.Withdraw(1000);
                else if (c == 2) { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine($"Balance: {acc.Balance}"); Console.ResetColor(); }
                else if (c == 3) acc.ShowTransactions();
                if (c != 4) Console.ReadKey();
            } while (c != 4);
        }

        static void Transfer(Account sender)
        {
            Console.Write("Receiver Acc: "); int rec = Convert.ToInt32(Console.ReadLine());
            var r = accounts.Find(a => a.AccountNumber == rec);
            if (r == null) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Receiver not found!"); Console.ResetColor(); return; }

            Console.Write("Amount: "); double amt = Convert.ToDouble(Console.ReadLine());
            if (amt <= sender.Balance)
            {
                sender.Balance -= amt;
                r.Balance += amt;
                sender.Transactions.Add(new Transaction { Type = "Transfer Sent", Amount = amt, Details = $"To {rec}", Date = DateTime.Now });
                r.Transactions.Add(new Transaction { Type = "Transfer Received", Amount = amt, Details = $"From {sender.AccountNumber}", Date = DateTime.Now });
                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Transfer Successful!"); Console.ResetColor();
            }
            else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Insufficient Balance!"); Console.ResetColor(); }
        }

        static void AdminPanel()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Admin Password: ");
            if (Console.ReadLine() != "admin123")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong Password!"); Console.ResetColor(); return;
            }
            Console.ResetColor();

            int ch;
            do
            {
                ShowHeader();
                Console.WriteLine("ADMIN PANEL");
                ShowMenu(new string[] {
                    "1. View All",
                    "2. View Details",
                    "3. Transactions",
                    "4. Delete",
                    "5. Search",
                    "6. Back" });
                ch = Convert.ToInt32(Console.ReadLine());

                if (ch >= 2 && ch <= 5)
                {
                    Account acc = FindAccount();
                    if (acc == null) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Not Found!"); Console.ResetColor(); }
                    else
                    {
                        if (ch == 2 || ch == 5) acc.ShowDetails();
                        else if (ch == 3) acc.ShowTransactions();
                        else if (ch == 4) { accounts.Remove(acc); Console.WriteLine("Deleted!"); }
                    }
                }
                else if (ch == 1)
                {
                    Console.WriteLine("Acc# | Name | Balance");
                    foreach (var a in accounts) Console.WriteLine($"{a.AccountNumber} | {a.Name} | {a.Balance}");
                }
                if (ch != 6) Console.ReadKey();
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
            string p = ""; ConsoleKeyInfo k;
            do
            {
                k = Console.ReadKey(true);
                if (k.Key != ConsoleKey.Enter && k.Key != ConsoleKey.Backspace)
                {
                    p += k.KeyChar;
                    Console.Write("*");
                }
                else if (k.Key == ConsoleKey.Backspace && p.Length > 0)
                {
                    p = p.Substring(0, (p.Length - 1));
                    Console.Write("\b \b");
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
