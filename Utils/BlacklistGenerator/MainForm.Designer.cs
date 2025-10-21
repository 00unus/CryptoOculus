namespace BlacklistGenerator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            resultsDataGridView = new DataGridView();
            Spread = new DataGridViewTextBoxColumn();
            Profit = new DataGridViewTextBoxColumn();
            Amount = new DataGridViewTextBoxColumn();
            BuyPair = new DataGridViewLinkColumn();
            SellPair = new DataGridViewLinkColumn();
            WithdrawNetworks = new DataGridViewTextBoxColumn();
            DepositNetworks = new DataGridViewTextBoxColumn();
            CompareResult = new DataGridViewTextBoxColumn();
            reloadButton = new Button();
            addSpread = new Button();
            addBuyExCoin = new Button();
            addCoinForAll = new Button();
            addSellExCoin = new Button();
            ((System.ComponentModel.ISupportInitialize)resultsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // resultsDataGridView
            // 
            resultsDataGridView.AllowUserToAddRows = false;
            resultsDataGridView.AllowUserToDeleteRows = false;
            resultsDataGridView.AllowUserToResizeColumns = false;
            resultsDataGridView.AllowUserToResizeRows = false;
            resultsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            resultsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resultsDataGridView.Columns.AddRange(new DataGridViewColumn[] { Spread, Profit, Amount, BuyPair, SellPair, WithdrawNetworks, DepositNetworks, CompareResult });
            resultsDataGridView.Location = new Point(12, 51);
            resultsDataGridView.MultiSelect = false;
            resultsDataGridView.Name = "resultsDataGridView";
            resultsDataGridView.ReadOnly = true;
            resultsDataGridView.RowHeadersVisible = false;
            resultsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            resultsDataGridView.Size = new Size(960, 398);
            resultsDataGridView.TabIndex = 0;
            resultsDataGridView.CellContentClick += ResultsDataGridView_CellContentClick;
            // 
            // Spread
            // 
            Spread.HeaderText = "Spread";
            Spread.Name = "Spread";
            Spread.ReadOnly = true;
            Spread.Width = 68;
            // 
            // Profit
            // 
            Profit.HeaderText = "Profit";
            Profit.Name = "Profit";
            Profit.ReadOnly = true;
            Profit.Width = 61;
            // 
            // Amount
            // 
            Amount.HeaderText = "Amount";
            Amount.Name = "Amount";
            Amount.ReadOnly = true;
            Amount.Width = 76;
            // 
            // BuyPair
            // 
            BuyPair.HeaderText = "BuyPair";
            BuyPair.Name = "BuyPair";
            BuyPair.ReadOnly = true;
            BuyPair.Resizable = DataGridViewTriState.True;
            BuyPair.SortMode = DataGridViewColumnSortMode.Automatic;
            BuyPair.Width = 72;
            // 
            // SellPair
            // 
            SellPair.HeaderText = "SellPair";
            SellPair.Name = "SellPair";
            SellPair.ReadOnly = true;
            SellPair.Resizable = DataGridViewTriState.True;
            SellPair.SortMode = DataGridViewColumnSortMode.Automatic;
            SellPair.Width = 70;
            // 
            // WithdrawNetworks
            // 
            WithdrawNetworks.HeaderText = "WithdrawNetworks";
            WithdrawNetworks.Name = "WithdrawNetworks";
            WithdrawNetworks.ReadOnly = true;
            WithdrawNetworks.Width = 133;
            // 
            // DepositNetworks
            // 
            DepositNetworks.HeaderText = "DepositNetworks";
            DepositNetworks.Name = "DepositNetworks";
            DepositNetworks.ReadOnly = true;
            DepositNetworks.Width = 122;
            // 
            // CompareResult
            // 
            CompareResult.HeaderText = "CompareResult";
            CompareResult.Name = "CompareResult";
            CompareResult.ReadOnly = true;
            CompareResult.Visible = false;
            CompareResult.Width = 113;
            // 
            // reloadButton
            // 
            reloadButton.Location = new Point(12, 14);
            reloadButton.Name = "reloadButton";
            reloadButton.Size = new Size(75, 23);
            reloadButton.TabIndex = 1;
            reloadButton.Text = "Reload";
            reloadButton.UseVisualStyleBackColor = true;
            reloadButton.Click += ReloadButton_Click;
            // 
            // addSpread
            // 
            addSpread.Location = new Point(897, 14);
            addSpread.Name = "addSpread";
            addSpread.Size = new Size(75, 23);
            addSpread.TabIndex = 2;
            addSpread.Text = "Add spread";
            addSpread.UseVisualStyleBackColor = true;
            addSpread.Click += AddSpread_Click;
            // 
            // addBuyExCoin
            // 
            addBuyExCoin.Location = new Point(786, 14);
            addBuyExCoin.Name = "addBuyExCoin";
            addBuyExCoin.Size = new Size(105, 23);
            addBuyExCoin.TabIndex = 3;
            addBuyExCoin.Text = "Add buy ex coin";
            addBuyExCoin.UseVisualStyleBackColor = true;
            addBuyExCoin.Click += AddBuyExCoin_Click;
            // 
            // addCoinForAll
            // 
            addCoinForAll.BackColor = SystemColors.Control;
            addCoinForAll.FlatStyle = FlatStyle.System;
            addCoinForAll.ForeColor = Color.Tomato;
            addCoinForAll.Location = new Point(559, 14);
            addCoinForAll.Name = "addCoinForAll";
            addCoinForAll.Size = new Size(110, 23);
            addCoinForAll.TabIndex = 4;
            addCoinForAll.Text = "Add coin for all";
            addCoinForAll.UseVisualStyleBackColor = false;
            addCoinForAll.Click += AddCoinForAll_Click;
            // 
            // addSellExCoin
            // 
            addSellExCoin.Location = new Point(675, 14);
            addSellExCoin.Name = "addSellExCoin";
            addSellExCoin.Size = new Size(105, 23);
            addSellExCoin.TabIndex = 5;
            addSellExCoin.Text = "Add sell ex coin";
            addSellExCoin.UseVisualStyleBackColor = true;
            addSellExCoin.Click += AddSellExCoin_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 461);
            Controls.Add(addSellExCoin);
            Controls.Add(addCoinForAll);
            Controls.Add(addBuyExCoin);
            Controls.Add(addSpread);
            Controls.Add(reloadButton);
            Controls.Add(resultsDataGridView);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Blacklist generator";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)resultsDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView resultsDataGridView;
        private Button reloadButton;
        private Button addSpread;
        private DataGridViewTextBoxColumn Spread;
        private DataGridViewTextBoxColumn Profit;
        private DataGridViewTextBoxColumn Amount;
        private DataGridViewLinkColumn BuyPair;
        private DataGridViewLinkColumn SellPair;
        private DataGridViewTextBoxColumn WithdrawNetworks;
        private DataGridViewTextBoxColumn DepositNetworks;
        private DataGridViewTextBoxColumn CompareResult;
        private Button addBuyExCoin;
        private Button addCoinForAll;
        private Button addSellExCoin;
    }
}
