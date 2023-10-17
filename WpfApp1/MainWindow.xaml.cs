// <copyright file="MainWindow.xaml.cs" company="Sofia">
// Copyright (c) Sofia. All rights reserved.
// </copyright>

namespace WpfApp1
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SQLite;
    using System.Windows;
    using System.Windows.Controls;

    public partial class MainWindow : Window
    {
        private ObservableCollection<JournalEntry> journalEntries;
        private int currentEntryIndex = 0;
        private int currentEntryID;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.journalEntries = new ObservableCollection<JournalEntry>();

            // Initialize SQLite database
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=journalApp.db; Version=3;"))
            {
                conn.Open();
                string sql = "CREATE TABLE IF NOT EXISTS entries (id INTEGER PRIMARY KEY, title TEXT, entry TEXT)";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
            }

            this.LoadEntries();  // Load entries from SQLite database
            this.EntryList.ItemsSource = this.journalEntries;

            // Event handlers
            this.SaveButton.Click += this.SaveButton_Click;
            this.ReadButton.Click += this.ReadButton_Click;
        }

        private void EntryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.EntryList.SelectedItem is JournalEntry selectedEntry)
            {
                this.JournalEntry.Text = selectedEntry.Content;
            }
        }

        private void LoadEntries()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=journalApp.db; Version=3;"))
            {
                conn.Open();
                string sql = "SELECT * FROM entries";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string? title = reader["title"] as string ?? string.Empty;  // Use null conditional and null coalescing
                    string? content = reader["entry"] as string ?? string.Empty;  // Use null conditional and null coalescing

                    this.journalEntries.Add(new JournalEntry
                    {
                        Title = title,
                        Content = content,
                    });
                }
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string entryTitle = this.EntryTitle.Text; // Assuming you have a TextBox named EntryTitle
            string entryText = this.JournalEntry.Text;

            // Save to SQLite database
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=journalApp.db; Version=3;"))
            {
                conn.Open();
                string sql = "INSERT INTO entries (title, entry) VALUES (@Title, @Entry)";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.Parameters.AddWithValue("@Title", entryTitle);
                command.Parameters.AddWithValue("@Entry", entryText);
                command.ExecuteNonQuery();
            }

            // Add to ObservableCollection
            this.journalEntries.Add(new JournalEntry { Title = entryTitle, Content = entryText });
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReadEntry(this.currentEntryIndex);
        }

        private void ReadEntry(int index)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=journalApp.db; Version=3;"))
            {
                conn.Open();
                string sql = $"SELECT * FROM entries ORDER BY id DESC LIMIT 1 OFFSET {index}";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.JournalEntry.Text = reader["entry"].ToString();
                    this.EntryTitle.Text = reader["title"].ToString(); // Assuming you have a TextBox named EntryTitle
                    this.currentEntryID = Convert.ToInt32(reader["id"]);
                }
            }
        }
    }
}
