using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;

namespace CountEmblems
{
    class Program
    {
        class Outline
        {
            public int Thickness;
            public System.Drawing.Color Color;
        }
        static Outline defaultOutline = new Outline { Thickness = 1, Color = System.Drawing.Color.Black };
        static List<Outline> outlines = new List<Outline>();
        static List<Outline> previousOutlines;
        // Previous amount of emblems
        static int previousTotal;
        // Previous error in the loop
        static string previousError;
        // 4 (version check) + 4 (playtime) + 1 (modified) + 1035 (maps visited)
        const int SKIPPED_BYTES = 1044;
        const int MAXEMBLEMS = 512;
        const int MAXEXTRAEMBLEMS = 16;
        const int NO_PREVIOUS_TOTAL = -1;
        const string PREVIOUS_INI_NAME = "previous.ini";
        const string CURRENT_INI_NAME = "current.ini";
        static string previousFileName;
        static string previousOutputName;
        static Form MainForm;
        static Form IOForm;
        static Form EditForm;
        //static System.Windows.Forms.Label emblemLabel;
        static System.Windows.Forms.Button button2FontColor;
        static System.Windows.Forms.Button button2FontColor2;
        static System.Windows.Forms.Button button2BackgroundColor;
        static System.Windows.Forms.TextBox gamedataBox;
        static TextBox outputBox;
        static System.Drawing.Color previousFontColor;
        static System.Drawing.Color previousFontColor2;
        static System.Drawing.Color previousBackColor;
        static System.Windows.Forms.Button button2OutlineColor;
        static Font previousFont;
        static int outlineCount = 0;
        static Button addOutlineButton;
        static Button removeOutlineButton;
        static ComboBox outlineMenu;
        static int previousIndex;
        static NumericUpDown thicknessUpDown;
        static Button buttonFontColor2;
        static NumericUpDown angleUpDown;
        static System.Windows.Forms.Button buttonOutlineColor;
        static ComboBox gradientMenu;
        static NumericUpDown textXUpDown;
        static NumericUpDown textYUpDown;
        static System.Drawing.Point previousTextLocation;
        static System.Drawing.Point textLocationMember = new System.Drawing.Point(30, 30);
        static System.Drawing.Point textLocation
        {
            get { return textLocationMember; }
            set
            {

                textLocationMember = value;
                if (textXUpDown != null && textYUpDown != null)
                {
                    textXUpDown.Value = textLocationMember.X;
                    textYUpDown.Value = textLocationMember.Y;
                }
                UpdateText();
            }
        }
        static PictureBox rectangleB;
        static PictureBox renderBox;
        static RectangleF rectangle;
        enum GradientOption
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Custom = 3
        };
        static GradientOption currentGradientOptionMember = GradientOption.None;
        static GradientOption currentGradientOption
        {
            get { return currentGradientOptionMember; }
            set
            {
                currentGradientOptionMember = value;
                if (gradientMenu != null)
                {
                    gradientMenu.SelectedIndex = (int)currentGradientOptionMember;
                }
            }
        }
        static GradientOption previousGradientOption;
        // The text to show
        static string textToShowMember;
        static string textToShow
        {
            get { return textToShowMember; }
            set
            {
                textToShowMember = value;
                UpdateText();
            }
        }

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

        // The output file name
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
        static System.Drawing.Color fontColorMember;
        static System.Drawing.Color fontColor
        {
            get { return fontColorMember; }
            set
            {
                fontColorMember = value;
                if (button2FontColor != null)
                {
                    button2FontColor.BackColor = value;
                }
                UpdateText();
            }
        }
        static System.Drawing.Color fontColor2Member;
        static System.Drawing.Color fontColor2
        {
            get { return fontColor2Member; }
            set
            {
                fontColor2Member = value;
                if (button2FontColor2 != null)
                {
                    button2FontColor2.BackColor = value;
                }
                UpdateText();
            }
        }
        // The background's color
        static System.Drawing.Color backColorMember;
        static System.Drawing.Color backColor
        {
            get { return backColorMember; }
            set
            {
                backColorMember = value;
                if (button2BackgroundColor != null)
                {
                    button2BackgroundColor.BackColor = value;
                }
                /*if (MainForm != null)
                {
                    MainForm.BackColor = value;
                }*/
                UpdateText();
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
                //if (emblemLabel != null)
                //{
                //    emblemLabel.Font = value;
                //}
                UpdateText();
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

        //The outline color
        static System.Drawing.Color outlineColorMember;
        static System.Drawing.Color outlineColor
        {
            get { return outlineColorMember; }
            set
            {
                outlineColorMember = value;
                if (button2OutlineColor != null)
                {
                    button2OutlineColor.BackColor = value;
                    if (outlines.Count > 0 && outlineMenu.SelectedIndex >= 0)
                    {
                        outlines[outlineMenu.SelectedIndex].Color = value;
                    }
                }
                UpdateText();
            }
        }

        //The outline thickness
        static int outlineThicknessMember;
        static int outlineThickness
        {
            get { return outlineThicknessMember; }
            set
            {
                outlineThicknessMember = value;
                if (thicknessUpDown != null)
                {
                    thicknessUpDown.Value = value;
                    if (outlines.Count > 0 && outlineMenu.SelectedIndex >= 0)
                    {
                        outlines[outlineMenu.SelectedIndex].Thickness = value;
                    }
                }
                UpdateText();
            }
        }
        static int previousCustomAngle;
        static int customAngleMember;
        static int customAngle
        {
            get { return customAngleMember; }
            set
            {
                customAngleMember = value;
                if (angleUpDown != null)
                {
                    angleUpDown.Value = value;
                    if (currentGradientOption == GradientOption.Custom)
                    {
                        //do the thing
                    }
                }
                UpdateText();
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
                    //if (emblemLabel.IsHandleCreated == true)
                    //{
                    //    emblemLabel.BeginInvoke(new MethodInvoker(delegate
                    //    {
                    //        emblemLabel.Text = total.ToString();
                    //    }));
                    //}
                    textToShow = total.ToString();

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

        static Form MakeForm(System.Drawing.Size size, System.Drawing.Color backcolor, string windowTitle)
        {
            Form form = new Form();
            form.ClientSize = size;
            form.MaximizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = backcolor;
            form.Text = windowTitle;
            //removed icon atm because i'm too lazy to do an other one lol
            //form.Icon = new Icon("icon.ico");
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
                DialogResult result = System.Windows.Forms.MessageBox.Show("There are unsaved changes. Do you want to save?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
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
        static void FormExit(object sender, FormClosingEventArgs e)
        {
            bool canceled = UnsavedChanges();
            if (canceled)
            {
                e.Cancel = true;
            }
            else
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

                int fA2 = fontColor2.A;
                int fR2 = fontColor2.R;
                int fG2 = fontColor2.G;
                int fB2 = fontColor2.B;
                string fontColorS2 = fA2 + "," + fR2 + "," + fG2 + "," + fB2;

                int gradient = (int)currentGradientOption;
                int angle = customAngle;
                string gradientS = gradient + "," + angle;

                int bA = backColor.A;
                int bR = backColor.R;
                int bG = backColor.G;
                int bB = backColor.B;
                string backColorS = bA + "," + bR + "," + bG + "," + bB;

                int width = windowWidth;
                int height = windowHeight;
                string size = width + "," + height;

                string location = textLocation.X + "," + textLocation.Y;

                int i;
                List<string> outlinesContent = new List<string>();
                //This next line is for the load file to know how many outlines there are to load
                outlinesContent.Add(outlines.Count.ToString());
                for (i = 0; i < outlines.Count; i++)
                {
                    outlinesContent.Add(outlines[i].Color.A + "," + outlines[i].Color.R + "," + outlines[i].Color.G + "," + outlines[i].Color.B + "," + outlines[i].Thickness);
                }

                FontConverter fc = new FontConverter();
                string[] contents = { fileName, outputName, fontColorS, fontColorS2, gradientS, backColorS, fc.ConvertToString(currentFont), size, location };
                File.WriteAllLines(file, contents);
                File.AppendAllLines(file, outlinesContent.ToArray());
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Couldn't save the layout file");
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
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Tick += (o, ea) => { UpdateText(); timer.Stop(); };
            timer.Interval = 100;
            timer.Start();

            updateGradientSettings();
            updateLocationSettings();
        }
        static void LoadFile(string file)
        {
            try
            {
                string[] contents = File.ReadAllLines(file);
                fileName = contents[0];
                gamedataBox.Text = fileName;
                outputName = contents[1];
                outputBox.Text = outputName;

                string[] fColor = contents[2].Split(',');
                int fA = Int32.Parse(fColor[0]);
                int fR = Int32.Parse(fColor[1]);
                int fG = Int32.Parse(fColor[2]);
                int fB = Int32.Parse(fColor[3]);
                fontColor = System.Drawing.Color.FromArgb(fA, fR, fG, fB);

                string[] fColor2 = contents[3].Split(',');
                int fA2 = Int32.Parse(fColor2[0]);
                int fR2 = Int32.Parse(fColor2[1]);
                int fG2 = Int32.Parse(fColor2[2]);
                int fB2 = Int32.Parse(fColor2[3]);
                fontColor2 = System.Drawing.Color.FromArgb(fA2, fR2, fG2, fB2);

                string[] gradientS = contents[4].Split(',');
                currentGradientOption = (GradientOption)Int32.Parse(gradientS[0]);
                customAngle = Int32.Parse(gradientS[1]);

                string[] bColor = contents[5].Split(',');
                int bA = Int32.Parse(bColor[0]);
                int bR = Int32.Parse(bColor[1]);
                int bG = Int32.Parse(bColor[2]);
                int bB = Int32.Parse(bColor[3]);
                backColor = System.Drawing.Color.FromArgb(bA, bR, bG, bB);

                FontConverter fc = new FontConverter();
                currentFont = fc.ConvertFromString(contents[6]) as Font;

                string[] size = contents[7].Split(',');
                windowWidth = Int32.Parse(size[0]);
                windowHeight = Int32.Parse(size[1]);
                textXUpDown.Maximum = windowWidth;
                textYUpDown.Maximum = windowHeight;

                string[] location = contents[8].Split(',');
                int X = Int32.Parse(location[0]);
                int Y = Int32.Parse(location[1]);
                textLocation = new System.Drawing.Point(X, Y);
                textXUpDown.Value = X;
                textYUpDown.Value = Y;

                outlines.Clear();
                int linenumber = 10;
                int outlinesCount = Int32.Parse(contents[9]);
                if (outlinesCount != 0)
                {
                    int i;
                    for (i = 0; i < outlinesCount; i++, linenumber++)
                    {
                        string[] outline = contents[linenumber].Split(',');
                        int A = Int32.Parse(outline[0]);
                        int R = Int32.Parse(outline[1]);
                        int G = Int32.Parse(outline[2]);
                        int B = Int32.Parse(outline[3]);
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(A, R, G, B);
                        int thickness = Int32.Parse(outline[4]);
                        outlines.Add(new Outline { Color = color, Thickness = thickness });
                    }
                }
            }
            catch
            {
                textXUpDown.Value = 30;
                textYUpDown.Value = 30;
                System.Windows.Forms.MessageBox.Show("Couldn't load the layout file");
            }
            //UpdateText();
        }

        static void MenuEdit_L(object sender, EventArgs e)
        {
            previousFontColor = fontColor;
            previousFontColor2 = fontColor2;
            previousGradientOption = currentGradientOption;
            previousCustomAngle = customAngle;
            button2FontColor.BackColor = fontColor;
            previousBackColor = backColor;
            button2BackgroundColor.BackColor = backColor;
            previousFont = currentFont;
            previousOutlines = new List<Outline>();
            previousTextLocation = textLocation;
            foreach (Outline outline in outlines)
            {
                previousOutlines.Add(new Outline { Color = outline.Color, Thickness = outline.Thickness });
            }
            resetOutlineMenu();
            updateGradientSettings();
            updateLocationSettings();
            EditForm.ShowDialog();
        }
        static void AddConstantLabel(string text, System.Drawing.Point location, Form form)
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = text;
            label.Location = location;
            label.AutoSize = true;
            form.Controls.Add(label);
        }
        static void BrowseGamedata(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog2 = new OpenFileDialog())
            {
                openFileDialog2.Filter = "Dat files (*.dat)|*.dat";
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
        }
        static void CancelIO(object sender, EventArgs e)
        {
            fileName = previousFileName;
            outputName = previousOutputName;
            Console.WriteLine("Gamedata : " + fileName);
        }
        enum fontColorId
        {
            FontColor1,
            FontColor2,
            Background,
            Outline
        }
        static void fontColorDialog(fontColorId colorId, System.Drawing.Color startColor)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.Color = startColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                switch (colorId)
                {
                    case fontColorId.FontColor1:
                        fontColor = colorDialog1.Color;
                        break;
                    case fontColorId.FontColor2:
                        fontColor2 = colorDialog1.Color;
                        break;
                    case fontColorId.Background:
                        backColor = colorDialog1.Color;
                        break;
                    case fontColorId.Outline:
                        outlineColor = colorDialog1.Color;
                        UpdateText();
                        break;
                }
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
            fontColor2 = previousFontColor2;
            currentGradientOption = previousGradientOption;
            customAngle = previousCustomAngle;
            backColor = previousBackColor;
            currentFont = previousFont;
            textLocation = previousTextLocation;
            outlines.Clear();
            foreach (Outline outline in previousOutlines)
            {
                outlines.Add(new Outline { Color = outline.Color, Thickness = outline.Thickness });
            }
            UpdateText();
        }

        static System.Windows.Forms.Button MakeButton(string text, System.Drawing.Point location, EventHandler eventHandler, Form form)
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button();
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
            //MainForm.BackColor = backColor;
            textXUpDown.Maximum = windowWidth;
            textYUpDown.Maximum = windowHeight;
        }
        static void UpdateText()
        {

            if (renderBox == null || !renderBox.IsHandleCreated)
            {
                return;
            }

            renderBox.BringToFront();

            Graphics formGraphics = renderBox.CreateGraphics();
            formGraphics.Clear(backColor);

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                if (currentFont != null)
                {
                    path.AddString(
                        textToShow,
                        currentFont.FontFamily,
                        (int)currentFont.Style,
                        formGraphics.DpiY * currentFont.Size / 72f,
                        textLocation,
                        StringFormat.GenericDefault);
                    rectangle = path.GetBounds();

                    //if(rectangleB != null)
                    //{
                    //    rectangleB.BeginInvoke(new MethodInvoker(delegate
                    //    {
                    //        rectangleB.Location = Rectangle.Round(rectangle).Location;
                    //        rectangleB.Size = Rectangle.Round(rectangle).Size;
                    //    }));
                    //}

                    formGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    formGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    formGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    System.Drawing.Drawing2D.LinearGradientBrush fillBrush;

                    switch (currentGradientOption)
                    {
                        case GradientOption.None:
                        default:
                            fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                new System.Drawing.Point((int)rectangle.Left, 0),
                                new System.Drawing.Point((int)rectangle.Right, 0),
                                fontColor,
                                fontColor);
                            break;
                        case GradientOption.Horizontal:
                            fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                new System.Drawing.Point((int)rectangle.Left, 0),
                                new System.Drawing.Point((int)rectangle.Right, 0),
                                fontColor,
                                fontColor2);
                            break;
                        case GradientOption.Vertical:
                            fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                new System.Drawing.Point(0, (int)rectangle.Top),
                                new System.Drawing.Point(0, (int)rectangle.Bottom),
                                fontColor,
                                fontColor2);
                            break;
                        case GradientOption.Custom:
                            // just a temp brush
                            fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                rectangle,
                                fontColor,
                                fontColor2,
                                customAngle);
                            break;
                    }



                    for (int i = outlines.Count - 1; i >= 0; i--)
                    {
                        int totalThickness = 0;
                        for (int j = i; j >= 0; j--)
                        {
                            totalThickness += outlines[j].Thickness;
                        }
                        formGraphics.DrawPath(new System.Drawing.Pen(outlines[i].Color, totalThickness), path);
                    }
                    formGraphics.FillPath(fillBrush, path);
                    fillBrush.Dispose();
                }
            }
            formGraphics.Dispose();

        }
        static void updateRemoveButton()
        {
            if (outlines.Count <= 0)
            {
                removeOutlineButton.Enabled = false;
            }
            else
            {
                removeOutlineButton.Enabled = true;
            }
        }


        static void updateOutlineSettings()
        {
            updateRemoveButton();
            thicknessUpDown.Enabled = false;
            buttonOutlineColor.Enabled = false;
            if (outlines.Count > 0 && outlineMenu.SelectedIndex >= 0)
            {
                thicknessUpDown.Enabled = true;
                buttonOutlineColor.Enabled = true;
                outlineColor = outlines[outlineMenu.SelectedIndex].Color;
                thicknessUpDown.Value = outlines[outlineMenu.SelectedIndex].Thickness;
            }
        }

        static void resetOutlineMenu()
        {
            outlineMenu.Items.Clear();
            if (outlines.Count > 0)
            {
                // This function is just for the visual menu
                for (int i = 0; i < outlines.Count; i++)
                {
                    outlineMenu.Items.Add("Outline " + (i + 1));
                }
                outlineMenu.SelectedIndex = 0;
            }
            updateOutlineSettings();
        }
        static void updateOutlineMenu()
        {
            if (outlines.Count > 0)
            {
                // This function is just for the visual menu
                for (int i = 0; i < outlineMenu.Items.Count; i++)
                {
                    outlineMenu.Items[i] = "Outline " + (i + 1);
                }
            }
            updateOutlineSettings();
        }
        static void updateLocationSettings()
        {
            if (textXUpDown != null && textYUpDown != null)
            {
                int x = textLocation.X;
                int y = textLocation.Y;
                textXUpDown.Value = x;
                textYUpDown.Value = y;
            }
        }
        static void newOutline(object sender, EventArgs e)
        {
            outlines.Add(new Outline { Thickness = defaultOutline.Thickness, Color = defaultOutline.Color });
            outlineMenu.Items.Add("Outline " + outlineCount);
            outlineMenu.SelectedIndex = outlines.Count - 1;
            updateOutlineMenu();
        }
        static void delOutline(object sender, EventArgs e)
        {
            if (outlineMenu.SelectedIndex > 1)
            {
                previousIndex = outlineMenu.SelectedIndex - 1;
            }
            else
            {
                previousIndex = 0;
            }
            outlines.RemoveAt(outlineMenu.SelectedIndex);
            outlineMenu.Items.RemoveAt(outlineMenu.SelectedIndex);
            if (outlines.Count > 0)
            {
                outlineMenu.SelectedIndex = previousIndex;
            }
            updateOutlineMenu();
        }
        static void thicknessChanged(object sender, EventArgs e)
        {
            outlineThickness = (int)thicknessUpDown.Value;
            UpdateText();
        }
        static void angleChanged(object sender, EventArgs e)
        {
            customAngle = (int)angleUpDown.Value;
        }
        static void LocationChanged()
        {
            textLocation = new System.Drawing.Point((int)textXUpDown.Value, (int)textYUpDown.Value);
            //rectangleB.Location = new System.Drawing.Point((int)textXUpDown.Value, (int)textYUpDown.Value);
        }
        static void updateGradientSettings()
        {
            currentGradientOption = (GradientOption)gradientMenu.SelectedIndex;
            buttonFontColor2.Enabled = false;
            angleUpDown.Enabled = false;
            if (currentGradientOption != GradientOption.None)
            {
                buttonFontColor2.Enabled = true;
            }
            if (currentGradientOption == GradientOption.Custom)
            {
                angleUpDown.Enabled = true;
            }
            UpdateText();
        }
        static bool movingStarted = false;
        static System.Drawing.Point startDelta;
        static void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int cursorX = MainForm.PointToClient(Cursor.Position).X;
            int cursorY = MainForm.PointToClient(Cursor.Position).Y;
            if (e.Button == MouseButtons.Left && cursorX >= rectangle.Left && cursorX <= rectangle.Right && cursorY >= rectangle.Top && cursorY <= rectangle.Bottom)
            {
                if (MainForm.ContextMenuStrip != null)
                {
                    if (MainForm.ContextMenuStrip.Visible)
                    {
                        movingStarted = false;
                    }
                }
                else if (MainForm.Focused)
                {
                    movingStarted = true;
                }
                startDelta = new System.Drawing.Point(
                    MainForm.PointToClient(Cursor.Position).X - textLocation.X,
                    MainForm.PointToClient(Cursor.Position).Y - textLocation.Y);
            }
        }
        static void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (movingStarted)
            {
                rectangleB.Location = new System.Drawing.Point(
                    MainForm.PointToClient(Cursor.Position).X - startDelta.X,
                    MainForm.PointToClient(Cursor.Position).Y - startDelta.Y);
                int x = Clamp(rectangleB.Location.X, (int)textXUpDown.Minimum, (int)textXUpDown.Maximum - 64);
                int y = Clamp(rectangleB.Location.Y, (int)textYUpDown.Minimum, (int)textYUpDown.Maximum - 64);
                textLocation = new System.Drawing.Point(x, y);
                //rectangleB.Location = textLocation;
                movingStarted = true;
            }
        }
        static void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            movingStarted = false;
        }
        static int Clamp(int value, int minValue, int maxValue)
        {
            if (value > maxValue)
            {
                value = maxValue;
            }
            if (value < minValue)
            {
                value = minValue;
            }
            return value;
        }
        [STAThread]
        static void Main()
        {
            MainForm = MakeForm(new System.Drawing.Size(250, 200), System.Drawing.Color.FromArgb(0, 0, 0), "Text File Display");
            windowWidth = 250;
            windowHeight = 200;
            MainForm.FormBorderStyle = FormBorderStyle.Sizable;
            MainForm.SizeGripStyle = SizeGripStyle.Hide;
            IOForm = MakeForm(new System.Drawing.Size(295, 175), System.Drawing.Color.FromArgb(240, 240, 240), "I/O Options");
            EditForm = MakeForm(new System.Drawing.Size(245, 350), System.Drawing.Color.FromArgb(240, 240, 240), "Edit Layout");
            MainForm.FormClosing += FormExit;
            MainForm.ResizeEnd += ResizeEnd;

            textToShow = "No text\nto display";
            currentFont = new Font("Arial", 20, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);

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

            EventHandler fontHandler = new EventHandler(fontDialog);

            EventHandler newOutlineHandler = new EventHandler(newOutline);
            EventHandler delOutlineHandler = new EventHandler(delOutline);
            EventHandler thicknessChangedHandler = new EventHandler(thicknessChanged);

            EventHandler angleChangedHandler = new EventHandler(angleChanged);

            EventHandler OkEditHandler = new EventHandler(OkEdit);
            EventHandler CancelEditHandler = new EventHandler(CancelEdit);

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("Text Input file", IO_Options);
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
                while (true)
                {
                    Analyze_file(fileName, outputName);
                    Thread.Sleep(100);
                }
            }).Start();

            AddConstantLabel("Input path:", new System.Drawing.Point(10, 10), IOForm);
            gamedataBox = new System.Windows.Forms.TextBox();
            gamedataBox.Text = fileName;
            gamedataBox.Location = new System.Drawing.Point(10, 30);
            gamedataBox.Size = new System.Drawing.Size(180, 20);
            IOForm.Controls.Add(gamedataBox);
            System.Windows.Forms.Button gamedataButton = MakeButton("Browse", new System.Drawing.Point(200, 30), gamedataButtonHandler, IOForm);

            AddConstantLabel("Output file's path:", new System.Drawing.Point(10, 60), IOForm);
            outputBox = new TextBox();
            outputBox.Text = outputName;
            outputBox.Location = new System.Drawing.Point(10, 80);
            outputBox.Size = new System.Drawing.Size(180, 20);
            IOForm.Controls.Add(outputBox);
            Button outputButton = MakeButton("Browse", new System.Drawing.Point(200, 80), outputButtonHandler, IOForm);

            System.Windows.Forms.Button buttonOkIO = MakeButton("OK", new System.Drawing.Point(110, 140), OkIOHandler, IOForm);
            buttonOkIO.DialogResult = DialogResult.OK;
            IOForm.AcceptButton = buttonOkIO;

            System.Windows.Forms.Button buttonCancelIO = MakeButton("Cancel", new System.Drawing.Point(200, 140), CancelIOHandler, IOForm);
            buttonCancelIO.DialogResult = DialogResult.Cancel;
            IOForm.CancelButton = buttonCancelIO;

            AddConstantLabel("Font color : ", new System.Drawing.Point(10, 15), EditForm);
            AddConstantLabel("Background color :  ", new System.Drawing.Point(10, 35), EditForm);
            AddConstantLabel("Font : ", new System.Drawing.Point(10, 55), EditForm);

            System.Windows.Forms.Button buttonFontColor = MakeButton("...", new System.Drawing.Point(155, 10), (o, e) => { fontColorDialog(fontColorId.FontColor1, fontColor); }, EditForm);

            button2FontColor = new System.Windows.Forms.Button();
            button2FontColor.Text = "";
            button2FontColor.Location = new System.Drawing.Point(130, 10);
            button2FontColor.Size = new System.Drawing.Size(23, 23);
            button2FontColor.Enabled = false;
            EditForm.Controls.Add(button2FontColor);

            System.Windows.Forms.Button buttonBackgroundColor = MakeButton("...", new System.Drawing.Point(155, 30), (o, e) => { fontColorDialog(fontColorId.Background, backColor); }, EditForm);

            button2BackgroundColor = new System.Windows.Forms.Button();
            button2BackgroundColor.Text = "";
            button2BackgroundColor.Location = new System.Drawing.Point(130, 30);
            button2BackgroundColor.Size = new System.Drawing.Size(23, 23);
            button2BackgroundColor.Enabled = false;
            EditForm.Controls.Add(button2BackgroundColor);

            System.Windows.Forms.Button buttonFont = MakeButton("...", new System.Drawing.Point(155, 50), fontHandler, EditForm);

            backColor = MainForm.BackColor;

            AddConstantLabel("Outlines:", new System.Drawing.Point(10, 95), EditForm);
            addOutlineButton = MakeButton("Add", new System.Drawing.Point(10, 110), newOutlineHandler, EditForm);
            addOutlineButton.Size = new System.Drawing.Size(40, 23);
            removeOutlineButton = MakeButton("Rem", new System.Drawing.Point(55, 110), delOutlineHandler, EditForm);
            removeOutlineButton.Size = new System.Drawing.Size(40, 23);
            updateRemoveButton();

            outlineMenu = new ComboBox();
            outlineMenu.Location = new System.Drawing.Point(10, 140);
            outlineMenu.Size = new System.Drawing.Size(87, 22);
            outlineMenu.DropDownStyle = ComboBoxStyle.DropDownList;
            EditForm.Controls.Add(outlineMenu);
            outlineMenu.SelectedValueChanged += (o, e) => { updateOutlineSettings(); };

            AddConstantLabel("Color:", new System.Drawing.Point(130, 115), EditForm);
            buttonOutlineColor = MakeButton("...", new System.Drawing.Point(194, 110), (o, e) => { fontColorDialog(fontColorId.Outline, outlineColor); }, EditForm);
            buttonOutlineColor.Enabled = false;
            buttonOutlineColor.Size = new System.Drawing.Size(35, 23);
            button2OutlineColor = new System.Windows.Forms.Button();
            button2OutlineColor.Text = "";
            button2OutlineColor.Location = new System.Drawing.Point(169, 110);
            button2OutlineColor.Size = new System.Drawing.Size(23, 23);
            button2OutlineColor.Enabled = false;
            EditForm.Controls.Add(button2OutlineColor);

            AddConstantLabel("Thickness:", new System.Drawing.Point(130, 143), EditForm);
            thicknessUpDown = new NumericUpDown();
            thicknessUpDown.Location = new System.Drawing.Point(190, 140);
            thicknessUpDown.Size = new System.Drawing.Size(40, 0);
            thicknessUpDown.ValueChanged += thicknessChangedHandler;
            thicknessUpDown.Enabled = false;
            EditForm.Controls.Add(thicknessUpDown);

            Label separator = new Label();
            separator.Location = new System.Drawing.Point(115, 90);
            separator.Width = 2;
            separator.Size = new System.Drawing.Size(1, 77);
            separator.BorderStyle = BorderStyle.Fixed3D;
            EditForm.Controls.Add(separator);
            separator.Enabled = false;
            Label separator2 = new Label();
            separator2.Location = new System.Drawing.Point(10, 167);
            separator2.Height = 2;
            separator2.Size = new System.Drawing.Size(220, 1);
            separator2.BorderStyle = BorderStyle.Fixed3D;
            EditForm.Controls.Add(separator2);
            separator2.Enabled = false;
            Label separator3 = new Label();
            separator3.Location = new System.Drawing.Point(10, 90);
            separator3.Height = 2;
            separator3.Size = new System.Drawing.Size(220, 1);
            separator3.BorderStyle = BorderStyle.Fixed3D;
            EditForm.Controls.Add(separator3);
            separator3.Enabled = false;

            AddConstantLabel("Font fill gradient", new System.Drawing.Point(10, 183), EditForm);
            gradientMenu = new ComboBox();
            gradientMenu.DropDownStyle = ComboBoxStyle.DropDownList;
            gradientMenu.Items.Insert((int)GradientOption.None, "None");
            gradientMenu.Items.Insert((int)GradientOption.Horizontal, "Horizontal");
            gradientMenu.Items.Insert((int)GradientOption.Vertical, "Vertical");
            gradientMenu.Items.Insert((int)GradientOption.Custom, "Custom");
            gradientMenu.Location = new System.Drawing.Point(110, 180);
            gradientMenu.SelectedIndex = (int)GradientOption.None;
            gradientMenu.SelectedValueChanged += (o, e) => { updateGradientSettings(); };
            EditForm.Controls.Add(gradientMenu);

            buttonFontColor2 = MakeButton("...", new System.Drawing.Point(155, 205), (o, e) => { fontColorDialog(fontColorId.FontColor2, fontColor2); }, EditForm);
            AddConstantLabel("Gradient End Color:", new System.Drawing.Point(10, 210), EditForm);
            button2FontColor2 = MakeButton("", new System.Drawing.Point(130, 205), null, EditForm);
            button2FontColor2.Size = new System.Drawing.Size(23, 23);
            button2FontColor2.Enabled = false;

            AddConstantLabel("Custom Angle:", new System.Drawing.Point(110, 238), EditForm);
            angleUpDown = new NumericUpDown();
            angleUpDown.Location = new System.Drawing.Point(190, 235);
            angleUpDown.Size = new System.Drawing.Size(40, 0);
            angleUpDown.ValueChanged += angleChangedHandler;
            angleUpDown.Enabled = false;
            EditForm.Controls.Add(angleUpDown);

            AddConstantLabel("Text location:", new System.Drawing.Point(10, 265), EditForm);
            AddConstantLabel("X:", new System.Drawing.Point(10, 288), EditForm);
            textXUpDown = new NumericUpDown();
            textXUpDown.Location = new System.Drawing.Point(30, 285);
            textXUpDown.Size = new System.Drawing.Size(70, 0);
            textXUpDown.ValueChanged += (o, e) => { LocationChanged(); };
            EditForm.Controls.Add(textXUpDown);
            AddConstantLabel("Y:", new System.Drawing.Point(110, 288), EditForm);
            textYUpDown = new NumericUpDown();
            textYUpDown.Location = new System.Drawing.Point(130, 285);
            textYUpDown.Size = new System.Drawing.Size(70, 0);
            textYUpDown.ValueChanged += (o, e) => { LocationChanged(); };
            EditForm.Controls.Add(textYUpDown);
            textXUpDown.Maximum = windowWidth;
            textYUpDown.Maximum = windowHeight;

            System.Windows.Forms.Button buttonOkEdit = MakeButton("Ok", new System.Drawing.Point(65, EditForm.Height - 65), OkEditHandler, EditForm);
            buttonOkEdit.DialogResult = DialogResult.OK;
            EditForm.AcceptButton = buttonOkEdit;

            System.Windows.Forms.Button buttonCancelEdit = MakeButton("Cancel", new System.Drawing.Point(155, EditForm.Height - 65), CancelEditHandler, EditForm);
            buttonCancelEdit.DialogResult = DialogResult.Cancel;
            EditForm.CancelButton = buttonCancelEdit;


            // Just some default values before loading
            fontColor = System.Drawing.Color.White;
            fontColor2 = System.Drawing.Color.White;

            Thread.CurrentThread.IsBackground = true;
            //MainForm.Controls.Add(emblemLabel);

            rectangleB = new PictureBox();
            rectangleB.Location = new System.Drawing.Point(textLocation.X, textLocation.Y);
            rectangleB.Size = Rectangle.Round(rectangle).Size;
            //rectangleB.MouseMove += OnMouseMove;
            MainForm.Controls.Add(rectangleB);

            renderBox = new PictureBox();
            renderBox.Size = new System.Drawing.Size(9001, 9001);
            renderBox.MouseDown += OnMouseDown;
            renderBox.MouseMove += OnMouseMove;
            renderBox.MouseUp += OnMouseUp;
            if (File.Exists(PREVIOUS_INI_NAME))
            {
                string previousFile = File.ReadAllText(PREVIOUS_INI_NAME);
                LoadFile(previousFile);
            }
            else
            {
                File.Create(PREVIOUS_INI_NAME).Close();
            }
            MainForm.SizeChanged += (o, e) => { MainFormSizeChanged(); };
            MainForm.Controls.Add(renderBox);
            MainForm.Paint += (o, e) => { UpdateText(); };
            Thread.CurrentThread.IsBackground = true;
            MainForm.ShowDialog();
        }

        private static void MainFormSizeChanged()
        {
            UpdateText();
        }
    }
}
