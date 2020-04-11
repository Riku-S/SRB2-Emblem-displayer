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
        const string PREVIOUS_INI_NAME = "previous.ini";
        const string CURRENT_INI_NAME = "current.ini";
        static string previousFileName;
        static string previousOutputName;
        static Form MainForm;
        static Form IOForm;
        static Form EditForm;
        static Label emblemLabel;
        static Button button2FontColor;
        static Button button2BackgroundColor;
        static TextBox gamedataBox;
        static TextBox outputBox;
        static Color previousFontColor;
        static Color previousBackColor;
        static Font previousFont;

        // The input file name
        static string fileNameMember;
        static string fileName
        {
            get { return fileNameMember; }
            set
            {
                fileNameMember = value;
                if (gamedataBox != null)
                {
                    gamedataBox.Text = value;
                }
            }
        }
        static string outputNameMember;
        static string outputName
        {
            get { return outputNameMember; }
            set
            {
                outputNameMember = value;
                if (outputBox != null)
                {
                    outputBox.Text = value;
                }
            }
        }
        // The font's color
        static Color fontColorMember;
        static Color fontColor
        {
            get { return fontColorMember; }
            set
            {
                fontColorMember = value;
                if (emblemLabel != null)
                {
                    emblemLabel.ForeColor = value;
                }
                if (button2FontColor != null)
                {
                    button2FontColor.BackColor = value;
                }
            }
        }
        // The background's color
        static Color backColorMember;
        static Color backColor
        {
            get { return backColorMember; }
            set
            {
                backColorMember = value;
                if (button2BackgroundColor != null)
                {
                    button2BackgroundColor.BackColor = value;
                }
                if (MainForm != null)
                {
                    MainForm.BackColor = value;
                }
            }
        }
        // The font
        static Font currentFontMember;
        static Font currentFont
        {
            get { return currentFontMember; }
            set
            {
                currentFontMember = value;
                if (emblemLabel != null)
                {
                    emblemLabel.Font = value;
                }
            }
        }

        static int windowWidthMember;
        static int windowWidth
        {
            get { return windowWidthMember; }
            set
            {
                windowWidthMember = value;
                if (MainForm != null)
                {
                    MainForm.Width = value;
                }
            }
        }
        static int windowHeightMember;
        static int windowHeight
        {
            get { return windowHeightMember; }
            set
            {
                windowHeightMember = value;
                if (MainForm != null)
                {
                    MainForm.Height = value;
                }
            }
        }

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
            if (fileName != null && fileName != "")
            {
                try
                {
                    bytes = File.ReadAllBytes(fileName);
                    // We don't want to read empty/corrupted gamedata
                    if (bytes.Length < SKIPPED_BYTES + MAXEMBLEMS + MAXEXTRAEMBLEMS && previousError != "short")
                    {
                        //Console.WriteLine("The gamedata is too short.");
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
                catch (FileNotFoundException e)
                {
                    string errorName = e.GetType().Name;
                    // We don't want error spam for every loop
                    if (previousError != errorName)
                    {
                        previousError = errorName;
                        MessageBox.Show(errorName + ": " + e.Message, errorName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch
                {

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
                    if (emblemLabel.IsHandleCreated == true)
                    {
                        emblemLabel.BeginInvoke(new MethodInvoker(delegate
                        {
                            emblemLabel.Text = total.ToString();
                        }));
                    }

                    if (outputName != null && outputName != "")
                    {
                        File.WriteAllText(outputName, total.ToString());
                    }

                    previousTotal = total;
                }
                catch (IOException e)
                {
                    string errorName = e.GetType().Name;
                    if (previousError != errorName)
                    {
                        previousError = errorName;
                        MessageBox.Show(errorName + ": " + e.Message, errorName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
        static Form MakeForm(Size size, Color backcolor, string windowTitle)
        {
            Form form = new Form();
            form.ClientSize = size;
            form.MaximizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = backcolor;
            form.Text = windowTitle;
            form.Icon = new Icon("icon.ico");
            form.ShowIcon = true;
            return form;
        }
        static bool UnsavedChanges()
        {
            bool canceled = false;
            SaveFile(CURRENT_INI_NAME);
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;
            string previousFile;
            previousFile = File.ReadAllText(PREVIOUS_INI_NAME);

            // Open the two files.
            fs1 = new FileStream(CURRENT_INI_NAME, FileMode.Open);
            try
            {
                fs2 = new FileStream(previousFile, FileMode.Open);
            }
            catch
            {
                fs2 = null;
            }
            if (fs2 != null)
            {
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));
                fs1.Close();
                fs2.Close();
            }
            else
            {
                file1byte = 0;
                file2byte = 1;
                fs1.Close();
            }

            File.Delete(CURRENT_INI_NAME);
            if ((file1byte - file2byte) != 0)
            {
                //MessageBox.Show("There are unsaved changes. Do you want to save?");
                DialogResult result = MessageBox.Show("There are unsaved changes. Do you want to save?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                switch (result)
                {
                    case DialogResult.Yes:
                        MenuSave();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        canceled = true;
                        break;
                    default:
                        canceled = true;
                        break;
                }
            }
            return canceled;
        }
        static void MenuExit(object sender, EventArgs e)
        {
            bool canceled = UnsavedChanges();
            if (!canceled)
            {
                Environment.Exit(0);
            }
        }
        static void MenuIO_Options(object sender, EventArgs e)
        {
            previousFileName = fileName;
            previousOutputName = outputName;
            Thread.CurrentThread.IsBackground = true;
            IOForm.ShowDialog();
        }
        static void MenuSave()
        {
            string previousFile;
            previousFile = File.ReadAllText(PREVIOUS_INI_NAME);
            if (previousFile != "")
            {
                SaveFile(previousFile);
            }
            else
            {
                using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                {
                    saveFileDialog1.Filter = "layout files (*.l)|*.l;";
                    saveFileDialog1.RestoreDirectory = true;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        SaveFile(saveFileDialog1.FileName);
                        File.WriteAllText(PREVIOUS_INI_NAME, saveFileDialog1.FileName);
                    }
                }
            }
        }
        static void MenuSave_L(object sender, EventArgs e)
        {
            MenuSave();
        }
        static void MenuSaveAs_L(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog3 = new SaveFileDialog())
            {
                saveFileDialog3.Filter = "layout files (*.l)|*.l;";
                saveFileDialog3.RestoreDirectory = true;
                if (saveFileDialog3.ShowDialog() == DialogResult.OK)
                {
                    SaveFile(saveFileDialog3.FileName);
                    File.WriteAllText(PREVIOUS_INI_NAME, saveFileDialog3.FileName);
                }
            }
        }
        static void SaveFile(string file)
        {
            try
            {
                int fA = fontColor.A;
                int fR = fontColor.R;
                int fG = fontColor.G;
                int fB = fontColor.B;
                string fontColorS = fA + "," + fR + "," + fG + "," + fB;

                int bA = backColor.A;
                int bR = backColor.R;
                int bG = backColor.G;
                int bB = backColor.B;
                string backColorS = bA + "," + bR + "," + bG + "," + bB;

                int width = windowWidth;
                int height = windowHeight;
                string size = width + "," + height;

                FontConverter fc = new FontConverter();
                string[] contents = { fileName, outputName, fontColorS, backColorS, fc.ConvertToString(currentFont), size };
                File.WriteAllLines(file, contents);
            }
            catch
            {
                MessageBox.Show("Couldn't save the layout file");
            }
        }
        static void MenuLoad_L(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                bool canceled = UnsavedChanges();
                if (!canceled)
                {
                    openFileDialog1.Filter = "layout files (*.l)|*.l;";
                    openFileDialog1.RestoreDirectory = true;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        LoadFile(openFileDialog1.FileName);
                        File.WriteAllText(PREVIOUS_INI_NAME, openFileDialog1.FileName);
                    }
                }
            }
        }
        static void LoadFile(string file)
        {
            try
            {
                string[] contents = File.ReadAllLines(file);
                fileName = contents[0];
                outputName = contents[1];
                gamedataBox.Text = fileName;
                outputBox.Text = outputName;

                string[] fColor = contents[2].Split(',');
                int fA = Int32.Parse(fColor[0]);
                int fR = Int32.Parse(fColor[1]);
                int fG = Int32.Parse(fColor[2]);
                int fB = Int32.Parse(fColor[3]);
                fontColor = Color.FromArgb(fA, fR, fG, fB);

                string[] bColor = contents[3].Split(',');
                int bA = Int32.Parse(bColor[0]);
                int bR = Int32.Parse(bColor[1]);
                int bG = Int32.Parse(bColor[2]);
                int bB = Int32.Parse(bColor[3]);
                backColor = Color.FromArgb(bA, bR, bG, bB);

                FontConverter fc = new FontConverter();
                currentFont = fc.ConvertFromString(contents[4]) as Font;

                string[] size = contents[5].Split(',');
                windowWidth = Int32.Parse(size[0]);
                windowHeight = Int32.Parse(size[1]);
            }
            catch
            {
                MessageBox.Show("Couldn't load the layout file");
            }
        }
        static void MenuEdit_L(object sender, EventArgs e)
        {
            previousFontColor = fontColor;
            button2FontColor.BackColor = fontColor;
            previousBackColor = backColor;
            button2BackgroundColor.BackColor = backColor;
            previousFont = currentFont;
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
            fileName = gamedataBox.Text;
            outputName = outputBox.Text;
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

        static void fontColorDialog(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.Color = emblemLabel.ForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                fontColor = colorDialog1.Color;
            }
        }
        static void backgroundColorDialog(object sender, EventArgs e)
        {
            ColorDialog colorDialog2 = new ColorDialog();
            colorDialog2.Color = MainForm.BackColor;
            if (colorDialog2.ShowDialog() == DialogResult.OK)
            {
                backColor = colorDialog2.Color;
            }
        }
        static void fontDialog(object sender, EventArgs e)
        {
            FontDialog fontDialog1 = new FontDialog();
            fontDialog1.Font = currentFont;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                currentFont = fontDialog1.Font;
            }
        }
        static void OkEdit(object sender, EventArgs e)
        {
        }
        static void CancelEdit(object sender, EventArgs e)
        {
            fontColor = previousFontColor;
            backColor = previousBackColor;
            currentFont = previousFont;
        }

        static Button MakeButton(string text, Point location, EventHandler eventHandler, Form form)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = location;
            button.Click += eventHandler;
            form.Controls.Add(button);
            return button;
        }
        static void ResizeEnd(object sender, EventArgs e)
        {
            windowHeight = MainForm.Height;
            windowWidth = MainForm.Width;
        }

        [STAThread]
        static void Main()
        {
            MainForm = MakeForm(new Size(250, 200), Color.FromArgb(0, 0, 0), "SRB2 Emblem Display");
            windowWidth = 250;
            windowHeight = 200;
            MainForm.FormBorderStyle = FormBorderStyle.Sizable;
            MainForm.SizeGripStyle = SizeGripStyle.Hide;
            IOForm = MakeForm(new Size(295, 175), Color.FromArgb(240, 240, 240), "I/O Options");
            EditForm = MakeForm(new Size(245, 155), Color.FromArgb(240, 240, 240), "Edit Layout");
            MainForm.FormClosing += MenuExit;
            MainForm.ResizeEnd += ResizeEnd;

            emblemLabel = new Label();
            emblemLabel.Text = "No text\nto display";
            emblemLabel.Location = new Point(10, 10);
            emblemLabel.AutoSize = true;
            emblemLabel.ForeColor = Color.FromArgb(255, 255, 255);
            currentFont = new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Point);

            EventHandler exitHandler = new EventHandler(MenuExit);
            EventHandler IO_Options = new EventHandler(MenuIO_Options);
            EventHandler Save_L = new EventHandler(MenuSave_L);
            EventHandler SaveAs_L = new EventHandler(MenuSaveAs_L);
            EventHandler Load_L = new EventHandler(MenuLoad_L);
            EventHandler Edit_L = new EventHandler(MenuEdit_L);

            EventHandler gamedataButtonHandler = new EventHandler(BrowseGamedata);
            EventHandler outputButtonHandler = new EventHandler(BrowseOutput);

            EventHandler OkIOHandler = new EventHandler(OkIO);
            EventHandler CancelIOHandler = new EventHandler(CancelIO);

            EventHandler fontColorHandler = new EventHandler(fontColorDialog);
            EventHandler backgroundColorHandler = new EventHandler(backgroundColorDialog);
            EventHandler fontHandler = new EventHandler(fontDialog);

            EventHandler OkEditHandler = new EventHandler(OkEdit);
            EventHandler CancelEditHandler = new EventHandler(CancelEdit);

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Input/Output options", IO_Options);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Save layout", Save_L);
            menu.MenuItems.Add("Save layout as...", SaveAs_L);
            menu.MenuItems.Add("Edit layout", Edit_L);
            menu.MenuItems.Add("Load layout", Load_L);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Exit", exitHandler);

            MainForm.ContextMenu = menu;

            // Take everythig out of this new thread and replace it with things that console application does
            new Thread(() =>
            {
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
            Button gamedataButton = MakeButton("Browse", new Point(200, 30), gamedataButtonHandler, IOForm);

            AddConstantLabel("Output file's path:", new Point(10, 60), IOForm);
            outputBox = new TextBox();
            outputBox.Text = outputName;
            outputBox.Location = new Point(10, 80);
            outputBox.Size = new Size(180, 20);
            IOForm.Controls.Add(outputBox);
            Button outputButton = MakeButton("Browse", new Point(200, 80), outputButtonHandler, IOForm);

            Button buttonOkIO = MakeButton("OK", new Point(110, 140), OkIOHandler, IOForm);
            buttonOkIO.DialogResult = DialogResult.OK;
            IOForm.AcceptButton = buttonOkIO;

            Button buttonCancelIO = MakeButton("Cancel", new Point(200, 140), CancelIOHandler, IOForm);
            buttonCancelIO.DialogResult = DialogResult.Cancel;
            IOForm.CancelButton = buttonCancelIO;

            AddConstantLabel("Font color : ", new Point(10, 15), EditForm);
            AddConstantLabel("Background color :  ", new Point(10, 35), EditForm);
            AddConstantLabel("Font : ", new Point(10, 55), EditForm);

            Button buttonFontColor = MakeButton("...", new Point(155, 10), fontColorHandler, EditForm);

            button2FontColor = new Button();
            button2FontColor.Text = "";
            button2FontColor.Location = new Point(130, 10);
            button2FontColor.Size = new Size(23, 23);
            button2FontColor.Enabled = false;
            EditForm.Controls.Add(button2FontColor);

            Button buttonBackgroundColor = MakeButton("...", new Point(155, 30), backgroundColorHandler, EditForm);

            button2BackgroundColor = new Button();
            button2BackgroundColor.Text = "";
            button2BackgroundColor.Location = new Point(130, 30);
            button2BackgroundColor.Size = new Size(23, 23);
            button2BackgroundColor.Enabled = false;
            EditForm.Controls.Add(button2BackgroundColor);

            Button buttonFont = MakeButton("...", new Point(155, 50), fontHandler, EditForm);

            Button buttonOkEdit = MakeButton("Ok", new Point(65, 120), OkEditHandler, EditForm);
            buttonOkEdit.DialogResult = DialogResult.OK;
            EditForm.AcceptButton = buttonOkEdit;

            Button buttonCancelEdit = MakeButton("Cancel", new Point(155, 120), CancelEditHandler, EditForm);
            buttonCancelEdit.DialogResult = DialogResult.Cancel;
            EditForm.CancelButton = buttonCancelEdit;

            backColor = MainForm.BackColor;
            fontColor = emblemLabel.ForeColor;

            Thread.CurrentThread.IsBackground = true;
            MainForm.Controls.Add(emblemLabel);

            if (File.Exists(PREVIOUS_INI_NAME))
            {
                string previousFile = File.ReadAllText(PREVIOUS_INI_NAME);
                LoadFile(previousFile);
            }
            else
            {
                File.Create(PREVIOUS_INI_NAME).Close();
            }

            MainForm.ShowDialog();
        }
    }
}