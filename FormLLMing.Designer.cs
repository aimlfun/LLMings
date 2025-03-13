using LLMing.components;

namespace LLMing
{
    partial class FormLLMing
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
            components = new System.ComponentModel.Container();
            buttonAskAI = new Button();
            panelInput = new Panel();
            textBoxQuestionToAskAI = new TextBox();
            panelSpacerBetweenQuestionAndButton = new Panel();
            panelButton = new Panel();
            panel4 = new Panel();
            panel1 = new Panel();
            buttonExamples = new Button();
            checkBoxOutputTranscript = new CheckBox();
            label2 = new Label();
            comboBoxModels = new ComboBox();
            buttonGuidance = new Button();
            buttonTools = new Button();
            openFileDialogGED = new OpenFileDialog();
            panelSplitter = new Panel();
            splitContainer1 = new SplitContainer();
            _chatWebBrowser = new ChatBrowser();
            _responseBrowser = new ResponseBrowser();
            toolTip1 = new ToolTip(components);
            panelInput.SuspendLayout();
            panelButton.SuspendLayout();
            panel4.SuspendLayout();
            panel1.SuspendLayout();
            panelSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // buttonAskAI
            // 
            buttonAskAI.Location = new Point(5, 15);
            buttonAskAI.Name = "buttonAskAI";
            buttonAskAI.Size = new Size(93, 52);
            buttonAskAI.TabIndex = 2;
            buttonAskAI.Text = "Ask";
            toolTip1.SetToolTip(buttonAskAI, "Click or press [Enter] to ask the question!");
            buttonAskAI.UseVisualStyleBackColor = true;
            buttonAskAI.Click += ButtonAskAI_Click;
            // 
            // panelInput
            // 
            panelInput.BackColor = Color.White;
            panelInput.Controls.Add(textBoxQuestionToAskAI);
            panelInput.Controls.Add(panelSpacerBetweenQuestionAndButton);
            panelInput.Controls.Add(panelButton);
            panelInput.Dock = DockStyle.Bottom;
            panelInput.Location = new Point(0, 682);
            panelInput.Margin = new Padding(5);
            panelInput.Name = "panelInput";
            panelInput.Padding = new Padding(10);
            panelInput.Size = new Size(1321, 105);
            panelInput.TabIndex = 16;
            // 
            // textBoxQuestionToAskAI
            // 
            textBoxQuestionToAskAI.BorderStyle = BorderStyle.FixedSingle;
            textBoxQuestionToAskAI.Dock = DockStyle.Fill;
            textBoxQuestionToAskAI.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxQuestionToAskAI.Location = new Point(10, 10);
            textBoxQuestionToAskAI.Multiline = true;
            textBoxQuestionToAskAI.Name = "textBoxQuestionToAskAI";
            textBoxQuestionToAskAI.PlaceholderText = "What would you like to know?";
            textBoxQuestionToAskAI.Size = new Size(1193, 85);
            textBoxQuestionToAskAI.TabIndex = 2;
            // 
            // panelSpacerBetweenQuestionAndButton
            // 
            panelSpacerBetweenQuestionAndButton.Dock = DockStyle.Right;
            panelSpacerBetweenQuestionAndButton.Location = new Point(1203, 10);
            panelSpacerBetweenQuestionAndButton.Name = "panelSpacerBetweenQuestionAndButton";
            panelSpacerBetweenQuestionAndButton.Size = new Size(10, 85);
            panelSpacerBetweenQuestionAndButton.TabIndex = 5;
            // 
            // panelButton
            // 
            panelButton.Controls.Add(buttonAskAI);
            panelButton.Dock = DockStyle.Right;
            panelButton.Location = new Point(1213, 10);
            panelButton.Name = "panelButton";
            panelButton.Size = new Size(98, 85);
            panelButton.TabIndex = 3;
            // 
            // panel4
            // 
            panel4.BackColor = Color.White;
            panel4.Controls.Add(panel1);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(1321, 44);
            panel4.TabIndex = 18;
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonExamples);
            panel1.Controls.Add(checkBoxOutputTranscript);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(comboBoxModels);
            panel1.Controls.Add(buttonGuidance);
            panel1.Controls.Add(buttonTools);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(889, 44);
            panel1.TabIndex = 5;
            // 
            // buttonExamples
            // 
            buttonExamples.Cursor = Cursors.Hand;
            buttonExamples.Location = new Point(466, 11);
            buttonExamples.Name = "buttonExamples";
            buttonExamples.Size = new Size(75, 33);
            buttonExamples.TabIndex = 8;
            buttonExamples.Text = "Examples";
            toolTip1.SetToolTip(buttonExamples, "View/Edit the \"Examples\" in Visual Code/Studio.");
            buttonExamples.UseVisualStyleBackColor = true;
            buttonExamples.Click += ButtonExamples_Click;
            // 
            // checkBoxOutputTranscript
            // 
            checkBoxOutputTranscript.AutoSize = true;
            checkBoxOutputTranscript.Cursor = Cursors.Hand;
            checkBoxOutputTranscript.Location = new Point(618, 19);
            checkBoxOutputTranscript.Name = "checkBoxOutputTranscript";
            checkBoxOutputTranscript.Size = new Size(141, 19);
            checkBoxOutputTranscript.TabIndex = 7;
            checkBoxOutputTranscript.Text = "Output Full Transcript";
            toolTip1.SetToolTip(checkBoxOutputTranscript, "Outputs everything sent to the LLM.");
            checkBoxOutputTranscript.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 21);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 6;
            label2.Text = "Model:";
            // 
            // comboBoxModels
            // 
            comboBoxModels.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxModels.FormattingEnabled = true;
            comboBoxModels.Location = new Point(50, 17);
            comboBoxModels.Name = "comboBoxModels";
            comboBoxModels.Size = new Size(250, 23);
            comboBoxModels.TabIndex = 5;
            // 
            // buttonGuidance
            // 
            buttonGuidance.Cursor = Cursors.Hand;
            buttonGuidance.Location = new Point(304, 11);
            buttonGuidance.Name = "buttonGuidance";
            buttonGuidance.Size = new Size(75, 33);
            buttonGuidance.TabIndex = 3;
            buttonGuidance.Text = "Guidance";
            toolTip1.SetToolTip(buttonGuidance, "Edit the \"Guidance\" in Visual Code/Studio.");
            buttonGuidance.UseVisualStyleBackColor = true;
            buttonGuidance.Click += ButtonGuidance_Click;
            // 
            // buttonTools
            // 
            buttonTools.Cursor = Cursors.Hand;
            buttonTools.Location = new Point(385, 11);
            buttonTools.Name = "buttonTools";
            buttonTools.Size = new Size(75, 33);
            buttonTools.TabIndex = 4;
            buttonTools.Text = "Tools Code";
            toolTip1.SetToolTip(buttonTools, "Edit the \"Tools.cs\" in Visual Code/Studio.");
            buttonTools.UseVisualStyleBackColor = true;
            buttonTools.Click += ButtonTools_Click;
            // 
            // openFileDialogGED
            // 
            openFileDialogGED.DefaultExt = "ged";
            openFileDialogGED.FileName = "*.ged";
            openFileDialogGED.Filter = "Genealogy files|*.ged";
            openFileDialogGED.Title = "Please select the .GED file you wish to ask questions about.";
            // 
            // panelSplitter
            // 
            panelSplitter.BackColor = Color.White;
            panelSplitter.Controls.Add(splitContainer1);
            panelSplitter.Dock = DockStyle.Fill;
            panelSplitter.Location = new Point(0, 44);
            panelSplitter.Name = "panelSplitter";
            panelSplitter.Padding = new Padding(10);
            panelSplitter.Size = new Size(1321, 638);
            panelSplitter.TabIndex = 19;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = Color.White;
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(10, 10);
            splitContainer1.Margin = new Padding(5);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(_chatWebBrowser);
            splitContainer1.Panel1.Margin = new Padding(5);
            splitContainer1.Panel1.Padding = new Padding(5);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(_responseBrowser);
            splitContainer1.Panel2.Margin = new Padding(5);
            splitContainer1.Panel2.Padding = new Padding(5);
            splitContainer1.Size = new Size(1301, 618);
            splitContainer1.SplitterDistance = 637;
            splitContainer1.SplitterWidth = 8;
            splitContainer1.TabIndex = 14;
            splitContainer1.TabStop = false;
            // 
            // _chatWebBrowser
            // 
            _chatWebBrowser.Dock = DockStyle.Fill;
            _chatWebBrowser.Location = new Point(5, 5);
            _chatWebBrowser.Name = "_chatWebBrowser";
            _chatWebBrowser.Size = new Size(625, 606);
            _chatWebBrowser.TabIndex = 0;
            // 
            // _responseBrowser
            // 
            _responseBrowser.Dock = DockStyle.Fill;
            _responseBrowser.Location = new Point(5, 5);
            _responseBrowser.Name = "_responseBrowser";
            _responseBrowser.Size = new Size(644, 606);
            _responseBrowser.TabIndex = 0;
            // 
            // FormLLMing
            // 
            AcceptButton = buttonAskAI;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1321, 787);
            Controls.Add(panelSplitter);
            Controls.Add(panel4);
            Controls.Add(panelInput);
            Name = "FormLLMing";
            StartPosition = FormStartPosition.WindowsDefaultBounds;
            Text = "LLMing Chat";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            panelInput.ResumeLayout(false);
            panelInput.PerformLayout();
            panelButton.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelSplitter.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button buttonAskAI;
        private Panel panelInput;
        private Panel panelButton;
        private Panel panelSpacerBetweenQuestionAndButton;
        private Panel panel4;
        private TextBox textBoxQuestionToAskAI;
        private OpenFileDialog openFileDialogGED;
        private Panel panelSplitter;
        private SplitContainer splitContainer1;
        private Button buttonTools;
        private Button buttonGuidance;
        private Panel panel1;
        private ComboBox comboBoxModels;
        private Label label2;
        private CheckBox checkBoxOutputTranscript;
        private ChatBrowser _chatWebBrowser;
        private ResponseBrowser _responseBrowser;
        private ToolTip toolTip1;
        private Button buttonExamples;
    }
}
