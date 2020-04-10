using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace CountEmblems
{
    class Program
    {
        // Previous amount of emblems
        static int previousTotal;
        // Previous error in the loop
        static string previousError;

        // 4 (version check) + 4 (playtime) + 1 (modified) + 1035 (maps visited)
        const int SKIPPED_BYTES = 1044;
        const int MAXEMBLEMS = 512;
        const int MAXEXTRAEMBLEMS = 16;
        const int EXIT_TIME = 10000;
        const int NO_PREVIOUS_TOTAL = -1;
        const string INI_NAME = "path.ini";
        static string fileName;
        static string outputName;
        static string previousFileName;
        static string previousOutputName;
        static Color fontColor;
        static Color backColor;
        static Form MainForm;
        static Form IOForm;
        static Form EditForm;
        static Label emblemLabel;
        static TextBox gamedataBox;
        static TextBox outputBox;
        static CheckBox checkBoxBold;
        static CheckBox checkBoxItalic;
        static CheckBox checkBoxUnderline;

        static byte ReadByte(ref byte[] bytes)
        {
            byte value = bytes[0];
            bytes = bytes.Skip(1).ToArray();
            return value;
        }
        static int CountEmblems(ref byte[] bytes, int max_emblems)
        {
            int result = 0;
            for (int i = 0; i < max_emblems;)
            {
                // Function directly copied from SRB2 source code, where the gamedata handling happens
                int j;
                byte rtemp = ReadByte(ref bytes);
                for (j = 0; j < 8 && j + i < max_emblems; ++j)
                    result += ((rtemp >> j) & 1);
                i += j;
            }
            return result;
        }
        static void Analyze_file(string fileName, string outputName)
        {
            byte[] bytes;
            int total = previousTotal;
            try
            {
                bytes = File.ReadAllBytes(fileName);
                // We don't want to read empty/corrupted gamedata
                if (bytes.Length < SKIPPED_BYTES + MAXEMBLEMS + MAXEXTRAEMBLEMS && previousError != "short")
                {
                    Console.WriteLine("The gamedata is too short.");
                    previousError = "short";
                }
                else
                {
                    bytes = bytes.Skip(SKIPPED_BYTES).ToArray();
                    int emblems = CountEmblems(ref bytes, MAXEMBLEMS);
                    int extraEmblems = CountEmblems(ref bytes, MAXEXTRAEMBLEMS);
                    total = emblems + extraEmblems;
                }
            }
            catch (IOException e)
            {
                string errorName = e.GetType().Name;
                // We don't want error spam for every loop
                if (previousError != errorName)
                {
                    Console.WriteLine("{0}: {1}", errorName, e.Message);
                    previousError = errorName;
                }
            }
            if (total == NO_PREVIOUS_TOTAL)
            {
                total = 0;
            }
            if (total != previousTotal)
            {
                try
                {
                    emblemLabel.Invoke(new MethodInvoker(delegate
                    {
                        emblemLabel.Text = total.ToString();
                    }));
                    File.WriteAllText(outputName, total.ToString());
                    previousTotal = total;
                    Console.WriteLine("Emblems: " + total);

                }
                catch (IOException e)
                {
                    string errorName = e.GetType().Name;
                    // We don't want error spam for every loop
                    if (previousError != errorName)
                    {
                        Console.WriteLine("{0}: {1}", errorName, e.Message);
                        previousError = errorName;
                    }
                }
            }
        }
        static void MenuExit(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        static void MenuIO_Options(object sender, EventArgs e)
        {
            previousFileName = fileName;
            previousOutputName = outputName;
            //new Thread(() =>
            //{
            Thread.CurrentThread.IsBackground = true;
            IOForm.ShowDialog();
            //}).Start();
        }
        static void MenuSave_L(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt;";
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    
                }
            }
        }
        static void MenuLoad_L(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog1.Filter = "txt files (*.txt)|*.txt;";
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.ShowDialog();

                if (openFileDialog1.FileName != "")
                {

                }
            }
        }
        static void MenuEdit_L(object sender, EventArgs e)
        {
            UpdateCheckBoxes();
            EditForm.ShowDialog();
        }
        static void AddConstantLabel(string text, Point location, Form form)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = location;
            label.AutoSize = true;
            form.Controls.Add(label);
        }
        static void BrowseGamedata(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog2 = new OpenFileDialog())
            {
                openFileDialog2.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog2.Filter = "dat files (*.dat)|*.dat;";
                openFileDialog2.RestoreDirectory = true;
                openFileDialog2.ShowDialog();

                if (openFileDialog2.FileName != "")
                {
                    fileName = openFileDialog2.FileName;
                    Console.WriteLine("Gamedata : " + fileName);
                    gamedataBox.Text = fileName;
                }
            }
        }
        static void BrowseOutput(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog2 = new SaveFileDialog())
            {
                saveFileDialog2.InitialDirectory = Directory.GetCurrentDirectory();
                saveFileDialog2.Filter = "txt files (*.txt)|*.txt;";
                saveFileDialog2.RestoreDirectory = true;
                saveFileDialog2.ShowDialog();

                if (saveFileDialog2.FileName != "")
                {
                    outputName = saveFileDialog2.FileName;
                    Console.WriteLine("Output file : " + outputName);
                    outputBox.Text = outputName;
                }
            }
        }
        static void OkIO(object sender, EventArgs e)
        {
            Console.WriteLine("Gamedata : " + fileName);
            Console.WriteLine("Output : " + outputName);
        }
        static void CancelIO(object sender, EventArgs e)
        {
            fileName = previousFileName;
            outputName = previousOutputName;
            Console.WriteLine("Gamedata : " + fileName);
            Console.WriteLine("Output : " + outputName);
        }
        static void UpdateCheckBoxes()
        {
            if (emblemLabel.Font.Bold)
            {
                checkBoxBold.Checked = true;
            }
            else
            {
                checkBoxBold.Checked = false;
            }
            if (emblemLabel.Font.Italic)
            {
                checkBoxItalic.Checked = true;
            }
            else
            {
                checkBoxItalic.Checked = false;
            }
            if (emblemLabel.Font.Underline)
            {
                checkBoxUnderline.Checked = true;
            }
            else
            {
                checkBoxUnderline.Checked = false;
            }
        }
        static void BoldChanged(object sender, EventArgs e)
        {
            CheckBox box = sender as CheckBox;
            if (emblemLabel.Font.Bold)
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style & (~FontStyle.Bold));
            }
            else
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style | FontStyle.Bold);
            }
            UpdateCheckBoxes();
        }
        static void ItalicChanged(object sender, EventArgs e)
        {
            CheckBox box = sender as CheckBox;
            if (emblemLabel.Font.Italic)
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style & (~FontStyle.Italic));
            }
            else
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style | FontStyle.Italic);
            }
            UpdateCheckBoxes();
        }
        static void UnderlineChanged(object sender, EventArgs e)
        {
            CheckBox box = sender as CheckBox;
            if (emblemLabel.Font.Underline)
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style & (~FontStyle.Underline));
            }
            else
            {
                emblemLabel.Font = new Font(emblemLabel.Font, emblemLabel.Font.Style | FontStyle.Underline);
            }
            UpdateCheckBoxes();
        }
        static void fontColorDialog(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.Color = emblemLabel.ForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                fontColor = colorDialog1.Color;
                emblemLabel.ForeColor = fontColor;
            }
        }
        static void backgroundColorDialog(object sender, EventArgs e)
        {
            ColorDialog colorDialog2 = new ColorDialog();
            colorDialog2.Color = MainForm.BackColor;
            if (colorDialog2.ShowDialog() == DialogResult.OK)
            {
                backColor = colorDialog2.Color;
                MainForm.BackColor = backColor;
            }
        }
        static void OkEdit(object sender, EventArgs e)
        {
            FontDialog fontDialog1 = new FontDialog();
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                emblemLabel.Font = fontDialog1.Font;
            }
        }
        static void CancelEdit(object sender, EventArgs e)
        {

        }

        [STAThread]
        static void Main()
        {
            MainForm = new Form();
            MainForm.ClientSize = new Size(1000, 200);
            MainForm.MinimizeBox = false;
            MainForm.MaximizeBox = false;
            MainForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            MainForm.StartPosition = FormStartPosition.CenterScreen;
            MainForm.BackColor = Color.FromArgb(0, 0, 0);
            MainForm.Text = "SRB2 Emblem Display";

            IOForm = new Form();
            IOForm.ClientSize = new Size(295, 175);
            IOForm.MinimizeBox = false;
            IOForm.MaximizeBox = false;
            IOForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            IOForm.StartPosition = FormStartPosition.CenterScreen;
            IOForm.BackColor = Color.FromArgb(240, 240, 240);
            IOForm.Text = "I/O Options";

            EditForm = new Form();
            EditForm.ClientSize = new Size(295, 175);
            EditForm.MinimizeBox = false;
            EditForm.MaximizeBox = false;
            EditForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            EditForm.StartPosition = FormStartPosition.CenterScreen;
            EditForm.BackColor = Color.FromArgb(240, 240, 240);
            EditForm.Text = "Edit Layout";

            emblemLabel = new Label();
            emblemLabel.Text = "You shouldn't see this";
            emblemLabel.Location = new Point(10, 10);
            emblemLabel.AutoSize = true;
            emblemLabel.Font = new Font("AzureoN", 20, FontStyle.Bold, GraphicsUnit.Point);

            EventHandler exitHandler = new EventHandler(MenuExit);
            EventHandler IO_Options = new EventHandler(MenuIO_Options);
            EventHandler Save_L = new EventHandler(MenuSave_L);
            EventHandler Load_L = new EventHandler(MenuLoad_L);
            EventHandler Edit_L = new EventHandler(MenuEdit_L);

            EventHandler gamedataButtonHandler = new EventHandler(BrowseGamedata);
            EventHandler outputButtonHandler = new EventHandler(BrowseOutput);

            EventHandler OkIOHandler = new EventHandler(OkIO);
            EventHandler CancelIOHandler = new EventHandler(CancelIO);

            EventHandler fontColorHandler = new EventHandler(fontColorDialog);
            EventHandler backgroundColorHandler = new EventHandler(backgroundColorDialog);
            EventHandler BoldChangedHandler = new EventHandler(BoldChanged);
            EventHandler ItalicChangedHandler = new EventHandler(ItalicChanged);
            EventHandler UnderlineChangedHandler = new EventHandler(UnderlineChanged);

            EventHandler OkEditHandler = new EventHandler(OkEdit);
            EventHandler CancelEditHandler = new EventHandler(CancelEdit);

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Input/Output options", IO_Options);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Save layout (not yet)", Save_L);
            menu.MenuItems.Add("Edit layout (wip)", Edit_L);
            menu.MenuItems.Add("Load layout (not yet)", Load_L);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Exit", exitHandler);

            MainForm.ContextMenu = menu;

            // Take everythig out of this new thread and replace it with things that console application does
            new Thread(() =>
            {
                fileName = "";
                outputName = "";
                try
                {
                    string[] lines = File.ReadAllLines(INI_NAME);
                    if (lines.Length < 2)
                    {
                        Console.WriteLine("The file {0} has too few lines! ", INI_NAME);
                        Thread.Sleep(EXIT_TIME);
                        return;
                    }
                    fileName = lines[0];
                    outputName = lines[1];
                }
                catch (IOException e)
                {
                    Console.WriteLine("{0}: Unable to read the paths file {1}", e.GetType().Name, INI_NAME);
                }
                if (fileName == "")
                {
                    Console.WriteLine("Input filename cannot be an empty string.");
                }
                if (outputName == "")
                {
                    Console.WriteLine("Output filename cannot be an empty string.");
                    Thread.Sleep(EXIT_TIME);
                    return;
                }
                // We want to change the number in the output file on the first loop
                previousTotal = NO_PREVIOUS_TOTAL;
                Console.WriteLine("Game data file: " + fileName);
                Console.WriteLine("Output file: " + outputName);
                while (true)
                {
                    Analyze_file(fileName, outputName);
                    Thread.Sleep(100);
                }
            }).Start();

            AddConstantLabel("Game data's path:", new Point(10, 10), IOForm);
            gamedataBox = new TextBox();
            gamedataBox.Text = fileName;
            gamedataBox.Location = new Point(10, 30);
            gamedataBox.Size = new Size(180, 20);
            IOForm.Controls.Add(gamedataBox);
            Button gamedataButton = new Button();
            gamedataButton.Text = "Browse";
            gamedataButton.Location = new Point(200, 30);
            gamedataButton.Click += gamedataButtonHandler;
            IOForm.Controls.Add(gamedataButton);

            AddConstantLabel("Output file's path:", new Point(10, 60), IOForm);
            outputBox = new TextBox();
            outputBox.Text = outputName;
            outputBox.Location = new Point(10, 80);
            outputBox.Size = new Size(180, 20);
            IOForm.Controls.Add(outputBox);
            Button outputButton = new Button();
            outputButton.Text = "Browse";
            outputButton.Location = new Point(200, 80);
            outputButton.Click += outputButtonHandler;
            IOForm.Controls.Add(outputButton);

            Button buttonOkIO = new Button();
            buttonOkIO.Text = "OK";
            buttonOkIO.Location = new Point(110, 140);
            buttonOkIO.DialogResult = DialogResult.OK;
            IOForm.Controls.Add(buttonOkIO);
            buttonOkIO.Click += OkIOHandler;
            IOForm.AcceptButton = buttonOkIO;

            Button buttonCancelIO = new Button();
            buttonCancelIO.Text = "Cancel";
            buttonCancelIO.Location = new Point(200, 140);
            buttonCancelIO.DialogResult = DialogResult.Cancel;
            IOForm.Controls.Add(buttonCancelIO);
            buttonCancelIO.Click += CancelIOHandler;
            IOForm.CancelButton = buttonCancelIO;

            AddConstantLabel("Font color : ", new Point(10, 10), EditForm);
            AddConstantLabel("Background color :  ", new Point(10, 30), EditForm);
            AddConstantLabel("Font : ", new Point(10, 50), EditForm);

            Button buttonFontColor = new Button();
            buttonFontColor.Text = "...";
            buttonFontColor.Location = new Point(200, 10);
            EditForm.Controls.Add(buttonFontColor);
            buttonFontColor.Click += fontColorHandler;

            Button button2FontColor = new Button();
            button2FontColor.Text = "";
            button2FontColor.Location = new Point(150, 10);
            EditForm.Controls.Add(button2FontColor);

            Button buttonBackgroundColor = new Button();
            buttonBackgroundColor.Text = "...";
            buttonBackgroundColor.Location = new Point(200, 30);
            EditForm.Controls.Add(buttonBackgroundColor);
            buttonBackgroundColor.Click += backgroundColorHandler;

            checkBoxBold = new CheckBox();
            checkBoxBold.Text = "B";
            checkBoxBold.Location = new Point(10, 70);
            checkBoxBold.Size = new Size(24, 24);
            checkBoxBold.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxBold.Appearance = Appearance.Button;
            checkBoxBold.Font = new Font(checkBoxBold.Font, FontStyle.Bold);
            checkBoxBold.Click += BoldChangedHandler;
            EditForm.Controls.Add(checkBoxBold);

            checkBoxItalic = new CheckBox();
            checkBoxItalic.Text = "I";
            checkBoxItalic.Location = new Point(40, 70);
            checkBoxItalic.Size = new Size(24, 24);
            checkBoxItalic.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxItalic.Appearance = Appearance.Button;
            checkBoxItalic.Font = new Font(checkBoxItalic.Font, FontStyle.Italic);
            checkBoxItalic.Click += ItalicChangedHandler;
            EditForm.Controls.Add(checkBoxItalic);

            checkBoxUnderline = new CheckBox();
            checkBoxUnderline.Text = "U";
            checkBoxUnderline.Location = new Point(70, 70);
            checkBoxUnderline.Size = new Size(24, 24);
            checkBoxUnderline.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxUnderline.Appearance = Appearance.Button;
            checkBoxUnderline.Font = new Font(checkBoxUnderline.Font, FontStyle.Underline);
            checkBoxUnderline.Click += UnderlineChangedHandler;
            EditForm.Controls.Add(checkBoxUnderline);

            Button buttonOkEdit = new Button();
            buttonOkEdit.Text = "OK";
            buttonOkEdit.Location = new Point(110, 140);
            buttonOkEdit.DialogResult = DialogResult.OK;
            EditForm.Controls.Add(buttonOkEdit);
            buttonOkEdit.Click += OkEditHandler;
            EditForm.AcceptButton = buttonOkEdit;

            Button buttonCancelEdit = new Button();
            buttonCancelEdit.Text = "Cancel";
            buttonCancelEdit.Location = new Point(200, 140);
            buttonCancelEdit.DialogResult = DialogResult.Cancel;
            EditForm.Controls.Add(buttonCancelEdit);
            buttonCancelEdit.Click += CancelEditHandler;
            EditForm.CancelButton = buttonCancelEdit;

            Thread.CurrentThread.IsBackground = true;
            MainForm.Controls.Add(emblemLabel);
            MainForm.ShowDialog();
        }
    }
}