using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace SimKimI
{
    public partial class MainForm : Form
    {
        public KimI Computer { get; protected set; }
        protected Assembler assembler;
        protected TapePunch tapePunch;
        protected TapeReader tapeReader;
        protected Boolean singleStepRead;

        public MainForm()
        {
            Font = new Font(Font.Name, 8.25f * 96f / CreateGraphics().DpiX, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
            InitializeComponent();
            Computer = new KimI();
            assembler = new Assembler();
            updateConfigScreen();
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = false;
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = false;
            BaudRate.Text = "1200";
            tapePunch = new TapePunch();
            punchButtons();
            tapeReader= new TapeReader();
            readerButtons();
        }

        public void Debug(String msg)
        {
            DebugOutput.AppendText(msg + "\r\n");
        }

        public void updateConfigScreen()
        {
            LoadFiles.Items.Clear();
            foreach (var item in Computer.LoadFiles) LoadFiles.Items.Add(item);
            BaseK1.Checked = Computer.BaseMemoryInstalled[1];
            BaseK2.Checked = Computer.BaseMemoryInstalled[2];
            BaseK3.Checked = Computer.BaseMemoryInstalled[3];
            BaseK4.Checked = Computer.BaseMemoryInstalled[4];
            BaseK1Rom.Checked = Computer.BaseRom[1];
            BaseK2Rom.Checked = Computer.BaseRom[2];
            BaseK3Rom.Checked = Computer.BaseRom[3];
            BaseK4Rom.Checked = Computer.BaseRom[4];
            ExtK1.Checked = Computer.ExtendedMemoryInstalled[1];
            ExtK2.Checked = Computer.ExtendedMemoryInstalled[2];
            ExtK3.Checked = Computer.ExtendedMemoryInstalled[3];
            ExtK4.Checked = Computer.ExtendedMemoryInstalled[4];
            ExtK5.Checked = Computer.ExtendedMemoryInstalled[5];
            ExtK6.Checked = Computer.ExtendedMemoryInstalled[6];
            ExtK7.Checked = Computer.ExtendedMemoryInstalled[7];
            ExtK1Rom.Checked = Computer.ExtendedRom[1];
            ExtK2Rom.Checked = Computer.ExtendedRom[2];
            ExtK3Rom.Checked = Computer.ExtendedRom[3];
            ExtK4Rom.Checked = Computer.ExtendedRom[4];
            ExtK5Rom.Checked = Computer.ExtendedRom[5];
            ExtK6Rom.Checked = Computer.ExtendedRom[6];
            ExtK7Rom.Checked = Computer.ExtendedRom[7];
            MemoryMapper.Checked = Computer.MemoryMapper;
            ExtK1.Enabled = MemoryMapper.Checked;
            ExtK2.Enabled = MemoryMapper.Checked;
            ExtK3.Enabled = MemoryMapper.Checked;
            ExtK4.Enabled = MemoryMapper.Checked;
            ExtK5.Enabled = MemoryMapper.Checked;
            ExtK6.Enabled = MemoryMapper.Checked;
            ExtK7.Enabled = MemoryMapper.Checked;
            ExtK1Rom.Enabled = MemoryMapper.Checked;
            ExtK2Rom.Enabled = MemoryMapper.Checked;
            ExtK3Rom.Enabled = MemoryMapper.Checked;
            ExtK4Rom.Enabled = MemoryMapper.Checked;
            ExtK5Rom.Enabled = MemoryMapper.Checked;
            ExtK6Rom.Enabled = MemoryMapper.Checked;
            ExtK7Rom.Enabled = MemoryMapper.Checked;
            if (Computer.Riot1IrqMode == ' ') Riot1IrqNone.Checked = true;
            if (Computer.Riot1IrqMode == 'I') Riot1IrqIrq.Checked = true;
            if (Computer.Riot1IrqMode == 'N') Riot1IrqNmi.Checked = true;
            if (Computer.Riot1IrqMode == 'R') Riot1IrqRst.Checked = true;
            if (Computer.Riot2IrqMode == ' ') Riot2IrqNone.Checked = true;
            if (Computer.Riot2IrqMode == 'I') Riot2IrqIrq.Checked = true;
            if (Computer.Riot2IrqMode == 'N') Riot2IrqNmi.Checked = true;
            if (Computer.Riot2IrqMode == 'R') Riot2IrqRst.Checked = true;
            if (Computer.UseKeypad) UserIOKeypad.Checked = true; else UserIOTelePrinter.Checked = true;
        }

        private void disgnosticsButton_Click(object sender, EventArgs e)
        {
            Diagnostics diags;
            diags = new Diagnostics(this);
            diags.Run();
        }

        private void assembleButton_Click(object sender, EventArgs e)
        {
            assembleButton.Enabled = false;
            assemblerOutput.Clear();
            assembler.showSymbolTable = SymbolTable.Checked;
            assembler.crossReference = CrossReference.Checked;
            assembler.outputMode = (MemoryRadioButton.Checked) ? 'M' : 'F';
            if (FileRadioButton.Checked) assembler.outputFile = HexFileName.Text;
            assembler.Assemble(assemblerInput.Lines,Computer.memory);
            assemblerOutput.AppendText(assembler.results);
            tabControl1.SelectedTab = asmOutputTab;
            assemblerOutput.Select(assemblerOutput.Text.Length, assemblerOutput.Text.Length);
            assemblerOutput.ScrollToCaret();
            assembleButton.Enabled = true;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            StreamWriter file;
            saveFileDialog.Filter = "Assembly source (*.asm)|*.asm|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                file = new StreamWriter(saveFileDialog.FileName);
                foreach (var line in assemblerInput.Lines)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            assemblerInput.Clear();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            StreamReader file;
            String line;
            assemblerInput.Clear();
            openFileDialog.Filter = "Assembly source (*.asm)|*.asm|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                file = new StreamReader(openFileDialog.FileName);
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    assemblerInput.AppendText(line + "\r\n");
                }
                file.Close();
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            int tag;
            tag = Convert.ToInt32(((Button)sender).Tag);
            if (tag == 1001)
            {
                Computer.cpu.Reset();
            }
            if (tag == 1002)
            {
                Computer.cpu.nmi();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Computer.cpu.Terminate();
        }

        private void Power_CheckedChanged(object sender, EventArgs e)
        {
            Computer.cpu.halted = !Power.Checked;
            if (Power.Checked) Computer.cpu.Reset();
        }

        private void SystemTimer_Tick(object sender, EventArgs e)
        {
            byte chr;
            int value;
            Address4.Value(Computer.Address[3]);
            Address3.Value(Computer.Address[2]);
            Address2.Value(Computer.Address[1]);
            Address1.Value(Computer.Address[0]);
            Data2.Value(Computer.Data[1]);
            Data1.Value(Computer.Data[0]);
            TapeCounter.Text = Computer.TapeDeck.TapeCounter().ToString();
            chr = Computer.TeleType.NextPrintByte();
            while (chr != 0)
            {
                TeleType.AppendText(((char)chr).ToString());
                if (tapePunch.Running) tapePunch.Punch(chr);
                chr = Computer.TeleType.NextPrintByte();
            }
            if (tapeReader.Running)
            {
                value = tapeReader.Next();
                if (value >= 0) Computer.TeleType.Send((byte)value);
            }
            if (Computer.cpu.Debug && !Computer.cpu.NextStep && !singleStepRead)
            {
                DebugA.Text = Computer.cpu.ac.ToString("X2");
                DebugX.Text = Computer.cpu.x.ToString("X2");
                DebugY.Text = Computer.cpu.y.ToString("X2");
                DebugPC.Text = Computer.cpu.pc.ToString("X2");
                DebugS.Text = Computer.cpu.sp.ToString("X2");
                FlagN.Visible = ((Computer.cpu.flags & 0x80) == 0x80);
                FlagV.Visible = ((Computer.cpu.flags & 0x40) == 0x40);
                FlagB.Visible = ((Computer.cpu.flags & 0x10) == 0x10);
                FlagD.Visible = ((Computer.cpu.flags & 0x08) == 0x08);
                FlagI.Visible = ((Computer.cpu.flags & 0x04) == 0x04);
                FlagZ.Visible = ((Computer.cpu.flags & 0x02) == 0x02);
                FlagC.Visible = ((Computer.cpu.flags & 0x01) == 0x01);
                DebugOutput.AppendText(Computer.cpu.DebugOutput + "\r\n");
                singleStepRead = true;
            }
        }

        private void KeypadMouseDown(object sender, MouseEventArgs e)
        {
            int tag;
            int row;
            int col;
            tag = Convert.ToInt32(((Button)sender).Tag);
            row = tag / 100;
            col = tag % 100;
            Computer.keypadRows[row] |= col;
        }

        private void KeypadMouseUp(object sender, MouseEventArgs e)
        {
            int tag;
            int row;
            int col;
            tag = Convert.ToInt32(((Button)sender).Tag);
            row = tag / 100;
            col = tag % 100;
            Computer.keypadRows[row] &= (~col);
        }

        private void HexFileName_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Intel Hex (*.hex)|*.hex|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                HexFileName.Text = saveFileDialog.FileName;
            }
        }

        private void LoadFilesAddButton_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Intel Hex (*.hex)|*.hex|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadFiles.Items.Add(openFileDialog.FileName);
                Computer.LoadFiles.Add(openFileDialog.FileName);
                Computer.SaveConfiguration();
            }
        }

        private void Base_CheckedChanged(object sender, EventArgs e)
        {
            int tag;
            tag = Convert.ToInt32(((CheckBox)sender).Tag);
            Computer.BaseMemoryInstalled[tag] = ((CheckBox)sender).Checked;
            Computer.SaveConfiguration();
        }

        private void BaseRom_CheckedChanged(object sender, EventArgs e)
        {
            int tag;
            tag = Convert.ToInt32(((CheckBox)sender).Tag);
            Computer.BaseRom[tag] = ((CheckBox)sender).Checked;
            Computer.SaveConfiguration();
        }

        private void Ext_CheckedChanged(object sender, EventArgs e)
        {
            int tag;
            tag = Convert.ToInt32(((CheckBox)sender).Tag);
            Computer.ExtendedMemoryInstalled[tag] = ((CheckBox)sender).Checked;
            Computer.SaveConfiguration();
        }

        private void ExtRom_CheckedChanged(object sender, EventArgs e)
        {
            int tag;
            tag = Convert.ToInt32(((CheckBox)sender).Tag);
            Computer.ExtendedRom[tag] = ((CheckBox)sender).Checked;
            Computer.SaveConfiguration();
        }

        private void MemoryMapper_CheckedChanged(object sender, EventArgs e)
        {
            Computer.MemoryMapper = MemoryMapper.Checked;
            Computer.SaveConfiguration();
            ExtK1.Enabled = MemoryMapper.Checked;
            ExtK2.Enabled = MemoryMapper.Checked;
            ExtK3.Enabled = MemoryMapper.Checked;
            ExtK4.Enabled = MemoryMapper.Checked;
            ExtK5.Enabled = MemoryMapper.Checked;
            ExtK6.Enabled = MemoryMapper.Checked;
            ExtK7.Enabled = MemoryMapper.Checked;
            ExtK1Rom.Enabled = MemoryMapper.Checked;
            ExtK2Rom.Enabled = MemoryMapper.Checked;
            ExtK3Rom.Enabled = MemoryMapper.Checked;
            ExtK4Rom.Enabled = MemoryMapper.Checked;
            ExtK5Rom.Enabled = MemoryMapper.Checked;
            ExtK6Rom.Enabled = MemoryMapper.Checked;
            ExtK7Rom.Enabled = MemoryMapper.Checked;
        }

        private void LoadFilesLoadAllNowButton_Click(object sender, EventArgs e)
        {
            Computer.ReloadFiles();
        }

        private void SST_CheckedChanged(object sender, EventArgs e)
        {
            Computer.SingleStep = SST.Checked;
        }

        private void Riot1Irq(object sender, EventArgs e)
        {
            if (Riot1IrqNone.Checked) Computer.SetRiot1Mode(' ');
            if (Riot1IrqIrq.Checked) Computer.SetRiot1Mode('I');
            if (Riot1IrqNmi.Checked) Computer.SetRiot1Mode('N');
            if (Riot1IrqRst.Checked) Computer.SetRiot1Mode('R');
        }

        private void Riot2Irq(object sender, EventArgs e)
        {
            if (Riot2IrqNone.Checked) Computer.SetRiot2Mode(' ');
            if (Riot2IrqIrq.Checked) Computer.SetRiot2Mode('I');
            if (Riot2IrqNmi.Checked) Computer.SetRiot2Mode('N');
            if (Riot2IrqRst.Checked) Computer.SetRiot2Mode('R');
        }

        private void LoadFilesDeleteButton_Click(object sender, EventArgs e)
        {
            if (LoadFiles.SelectedIndex >= 0)
            {
                Computer.LoadFiles.Remove((String)LoadFiles.Items[LoadFiles.SelectedIndex]);
                LoadFiles.Items.RemoveAt(LoadFiles.SelectedIndex);
                Computer.SaveConfiguration();
            }
        }

        private void EnableDebugMode_CheckedChanged(object sender, EventArgs e)
        {
            Computer.cpu.Debug = EnableDebugMode.Checked;
            Computer.cpu.NextStep = false;
        }

        private void DebugStepButton_Click(object sender, EventArgs e)
        {
            if (EnableDebugMode.Checked)
            {
                Computer.cpu.NextStep = true;
                singleStepRead = false;
            }
        }

        private void UserIOKeypad_CheckedChanged(object sender, EventArgs e)
        {
            Computer.UseKeypad = UserIOKeypad.Checked;
            Computer.SaveConfiguration();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Play();
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = false;
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Record();
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = false;
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void FastForwardButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.FastForward();
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = false;
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void RewindButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Rewind();
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = false;
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Stop();
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = true;
            PlayButton.Enabled = true;
            RecordButton.Enabled = true;
            FastForwardButton.Enabled = true;
            RewindButton.Enabled = true;
            StopButton.Enabled = false;
        }

        private void EjectButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Eject();
            InsertTapeButton.Enabled = true;
            EjectButton.Enabled = false;
            PlayButton.Enabled = false;
            RecordButton.Enabled = false;
            FastForwardButton.Enabled = false;
            RewindButton.Enabled = false;
            StopButton.Enabled = false;
            TapeFileName.Enabled = true;
        }

        private void InsertTapeButton_Click(object sender, EventArgs e)
        {
            Computer.TapeDeck.Mount(TapeFileName.Text);
            InsertTapeButton.Enabled = false;
            EjectButton.Enabled = true;
            PlayButton.Enabled = true;
            RecordButton.Enabled = true;
            FastForwardButton.Enabled = true;
            RewindButton.Enabled = true;
            StopButton.Enabled = false;
            TapeFileName.Enabled = false;
        }

        private void TapeFileName_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Tape File (*.tap)|*.tap|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                TapeFileName.Text = saveFileDialog.FileName;
                if (TapeFileName.Text.Length > 0)
                {
                    InsertTapeButton.Enabled = true;
                }
            }
        }

        private void TeletypeClearButton_Click(object sender, EventArgs e)
        {
            TeleType.Clear();
        }

        private void TeleType_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte key;
            key = (byte)e.KeyChar;
            if (ForceUpperCase.Checked && key >= 'a' && key <= 'z') key -= 32;
            Computer.TeleType.Send(key);
        }

        private void TeleType_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 0x2e) Computer.TeleType.Send(0x7f);
        }

        private void BaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rate = Convert.ToInt32(BaudRate.Text);
            Computer.TeleType.Baud = rate;
        }

        private void PunchFilename_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Paper Tape (*.ptp)|*.ptp|All Files (*.*)|*.*";
            saveFileDialog.FileName = "";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                PunchFilename.Text = saveFileDialog.FileName;
            }
        }

        private void punchButtons()
        {
            PunchMountButton.Enabled = !tapePunch.Mounted;
            PunchUnmountButton.Enabled = (tapePunch.Mounted && !tapePunch.Running);
            PunchStartButton.Enabled = (tapePunch.Mounted && !tapePunch.Running);
            PunchStopButton.Enabled = (tapePunch.Mounted && tapePunch.Running);
        }

        private void readerButtons()
        {
            ReaderMountButton.Enabled = !tapeReader.Mounted;
            ReaderUnmountButton.Enabled = (tapeReader.Mounted && !tapeReader.Running);
            ReaderStartButton.Enabled = (tapeReader.Mounted && !tapeReader.Running);
            ReaderStopButton.Enabled = (tapeReader.Mounted && tapeReader.Running);
        }

        private void PunchMountButton_Click(object sender, EventArgs e)
        {
            tapePunch.Mount(PunchFilename.Text);
            punchButtons();
        }

        private void PunchUnmountButton_Click(object sender, EventArgs e)
        {
            tapePunch.Unmount();
            punchButtons();
        }

        private void PunchStartButton_Click(object sender, EventArgs e)
        {
            tapePunch.Start();
            punchButtons();
        }

        private void PunchStopButton_Click(object sender, EventArgs e)
        {
            tapePunch.Stop();
            punchButtons();
        }

        private void ReaderFilename_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Paper Tape (*.ptp)|*.ptp|All Files (*.*)|*.*";
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ReaderFilename.Text = openFileDialog.FileName;
            }

        }

        private void ReaderMountButton_Click(object sender, EventArgs e)
        {
            tapeReader.Mount(ReaderFilename.Text);
            readerButtons();
        }

        private void ReaderUnmountButton_Click(object sender, EventArgs e)
        {
            tapeReader.Unmount();
            readerButtons();
        }

        private void ReaderStartButton_Click(object sender, EventArgs e)
        {
            tapeReader.Start();
            readerButtons();
        }

        private void ReaderStopButton_Click(object sender, EventArgs e)
        {
            tapeReader.Stop();
            readerButtons();
        }

        private void MemoryMap_CheckedChanged(object sender, EventArgs e)
        {
            assembler.showMemoryMap = MemoryMap.Checked;
        }
    }
}
