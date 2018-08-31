#define DEBUG
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using TimeAgo.Properties;

namespace TimeAgo
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

    public readonly Dictionary<string, string> LanguageDicoEn = new Dictionary<string, string>();
    public readonly Dictionary<string, string> LanguageDicoFr = new Dictionary<string, string>();
    public readonly Dictionary<string, string> DataFile = new Dictionary<string, string>();
    private string _currentLanguage = "english";
    private ConfigurationOptions _configurationOptions = new ConfigurationOptions();
    private readonly GlobalList AllEvent = new GlobalList();
    private bool DataFileHasBeenModified = false;

    private void QuitToolStripMenuItemClick(object sender, EventArgs e)
    {
      //We save settings before quitting
      SaveWindowValue();
      if (DataFileHasBeenModified)
      {
        SaveDataFile();
      }

      Application.Exit();
    }

    private void SaveDataFile()
    {
      //We save all new data to XML file
      try
      {
        if (File.Exists(Settings.Default.DataFileName))
        {
          File.Delete(Settings.Default.DataFileName);
        }
      }
      catch (Exception exception)
      {
        DisplayMessage($"Error while deleting {Settings.Default.DataFileName}, the exception is {exception.Message}", "Deletion Error", MessageBoxButtons.OK);
        return;
      }

      StringBuilder finalFile = new StringBuilder();
      finalFile.Append(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
      finalFile.Append(Environment.NewLine);
      finalFile.Append("<items>");
      finalFile.Append(Environment.NewLine);
      foreach (var item in listBoxMain.Items)
      {
        foreach (var subItem in AllEvent.GlobalListOfEvents[item.ToString()])
        {
          finalFile.Append(CreateTagNode(subItem.Title, subItem.DateOfEvent));
        }
      }

      finalFile.Append("</items>");
      finalFile.Append(Environment.NewLine);

      try
      {
        using (StreamWriter sw = new StreamWriter(Settings.Default.DataFileName))
        {
          sw.WriteLine(finalFile.ToString());
        }
      }
      catch (Exception exception)
      {
        DisplayMessage($"Error while saving the file  {Settings.Default.DataFileName}, the exception is {exception.Message}", "Save Error", MessageBoxButtons.OK);
        return;
      }

      DisplayMessage("The data file has been saved correctly", "File Saved", MessageBoxButtons.OK);
    }

    private static string CreateTagNode(string title, DateTime dateEvent)
    {
      StringBuilder result = new StringBuilder();
      result.Append("<item>");
      result.Append(Environment.NewLine);
      result.Append("<title>");
      result.Append(title);
      result.Append("</title>");
      result.Append(Environment.NewLine);
      result.Append("<date>");
      result.Append(dateEvent); // check if format is needed
      result.Append("</date>");
      result.Append(Environment.NewLine);
      result.Append("</item>");
      result.Append(Environment.NewLine);
      return result.ToString();
    }

    private void AboutToolStripMenuItemClick(object sender, EventArgs e)
    {
      // create a new instance of aboutBox and display it
      AboutBoxApplication aboutBoxApplication = new AboutBoxApplication();
      aboutBoxApplication.ShowDialog();
    }

    private void DisplayTitle()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      Text += $" V{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}.{fvi.FilePrivatePart}";
    }

    private void FormMainLoad(object sender, EventArgs e)
    {
      //We load all settings at the start of the application
      LoadSettingsAtStartup();
    }

    private void LoadSettingsAtStartup()
    {
      DisplayTitle();
      GetWindowValue();
      LoadLanguages();
      SetLanguage(Settings.Default.LastLanguageUsed);
      CheckDataFile();
      LoadList();
      dateTimePickerMain.Value = DateTime.Now;
    }

    private void LoadList()
    {
      // we load the XML data file into the AllListEvent variable
      string fileName = Settings.Default.DataFileName;
      XDocument xmlDoc;
      try
      {
        xmlDoc = XDocument.Load(fileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message);
        return;
      }

      var result = from node in xmlDoc.Descendants("item")
                   where node.HasElements
                   let xElementTitle = node.Element("title")
                   where xElementTitle != null
                   let xElementDate = node.Element("date")
                   where xElementDate != null
                   select new
                   {
                     titleValue = xElementTitle.Value,
                     dateValue = xElementDate.Value
                   };

      foreach (var q in result)
      {
        DateTime tmpDate = DateTime.Now;
        DateTime.TryParse(q.dateValue, out tmpDate);
        AllEvent.AddOneEvent(new Event(q.titleValue, tmpDate));
      }

      listBoxMain.Items.Clear();
      listBoxSubItems.Items.Clear();
      foreach (string item in AllEvent.GlobalListOfEvents.Keys.ToList())
      {
        listBoxMain.Items.Add(item);
      }
    }

    private static void CheckDataFile()
    {
      // check if data file is present, if not create it
      if (!File.Exists(Settings.Default.DataFileName))
      {
        CreateDataFile();
      }
    }

    private static void CreateDataFile()
    {
      List<string> minimumFile = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
        "<items>",
        "<item>",
        "<title>Title1</title>",
        "<date>04/07/2018</date>",
        "</item>",
        "</items>"
      };

      StreamWriter sw = new StreamWriter(Settings.Default.DataFileName);
      foreach (string item in minimumFile)
      {
        sw.WriteLine(item);
      }

      sw.Close();
    }

    private void LoadConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void SaveConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void LoadLanguages()
    {
      if (!File.Exists(Settings.Default.LanguageFileName))
      {
        CreateLanguageFile();
      }

      // read the translation file and feed the language
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(Settings.Default.LanguageFileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show(Resources.Error_while_loading_the + Punctuation.OneSpace +
          Settings.Default.LanguageFileName + Punctuation.OneSpace + Resources.XML_file +
          Punctuation.OneSpace + exception.Message);
        CreateLanguageFile();
        return;
      }

      var result = from node in xDoc.Descendants("term")
                   where node.HasElements
                   let xElementName = node.Element("name")
                   where xElementName != null
                   let xElementEnglish = node.Element("englishValue")
                   where xElementEnglish != null
                   let xElementFrench = node.Element("frenchValue")
                   where xElementFrench != null
                   select new
                   {
                     name = xElementName.Value,
                     englishValue = xElementEnglish.Value,
                     frenchValue = xElementFrench.Value
                   };
      foreach (var i in result)
      {
        if (!LanguageDicoEn.ContainsKey(i.name))
        {
          LanguageDicoEn.Add(i.name, i.englishValue);
        }
#if DEBUG
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like + Punctuation.Colon +
            Punctuation.OneSpace + i.name);
        }
#endif
        if (!LanguageDicoFr.ContainsKey(i.name))
        {
          LanguageDicoFr.Add(i.name, i.frenchValue);
        }
#if DEBUG
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like + Punctuation.Colon +
            Punctuation.OneSpace + i.name);
        }
#endif
      }
    }

    private static void CreateLanguageFile()
    {
      List<string> minimumVersion = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
        "<terms>",
         "<term>",
        "<name>MenuFile</name>",
        "<englishValue>File</englishValue>",
        "<frenchValue>Fichier</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileNew</name>",
        "<englishValue>New</englishValue>",
        "<frenchValue>Nouveau</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileOpen</name>",
        "<englishValue>Open</englishValue>",
        "<frenchValue>Ouvrir</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSave</name>",
        "<englishValue>Save</englishValue>",
        "<frenchValue>Enregistrer</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSaveAs</name>",
        "<englishValue>Save as ...</englishValue>",
        "<frenchValue>Enregistrer sous ...</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFilePrint</name>",
        "<englishValue>Print ...</englishValue>",
        "<frenchValue>Imprimer ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufilePageSetup</name>",
          "<englishValue>Page setup</englishValue>",
          "<frenchValue>Aperçu avant impression</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufileQuit</name>",
          "<englishValue>Quit</englishValue>",
          "<frenchValue>Quitter</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEdit</name>",
          "<englishValue>Edit</englishValue>",
          "<frenchValue>Edition</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCancel</name>",
          "<englishValue>Cancel</englishValue>",
          "<frenchValue>Annuler</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditRedo</name>",
          "<englishValue>Redo</englishValue>",
          "<frenchValue>Rétablir</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCut</name>",
          "<englishValue>Cut</englishValue>",
          "<frenchValue>Couper</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCopy</name>",
          "<englishValue>Copy</englishValue>",
          "<frenchValue>Copier</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditPaste</name>",
          "<englishValue>Paste</englishValue>",
          "<frenchValue>Coller</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditSelectAll</name>",
          "<englishValue>Select All</englishValue>",
          "<frenchValue>Sélectionner tout</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuTools</name>",
          "<englishValue>Tools</englishValue>",
          "<frenchValue>Outils</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsCustomize</name>",
          "<englishValue>Customize ...</englishValue>",
          "<frenchValue>Personaliser ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsOptions</name>",
          "<englishValue>Options</englishValue>",
          "<frenchValue>Options</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguage</name>",
          "<englishValue>Language</englishValue>",
          "<frenchValue>Langage</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageEnglish</name>",
          "<englishValue>English</englishValue>",
          "<frenchValue>Anglais</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageFrench</name>",
          "<englishValue>French</englishValue>",
          "<frenchValue>Français</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelp</name>",
          "<englishValue>Help</englishValue>",
          "<frenchValue>Aide</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSummary</name>",
          "<englishValue>Summary</englishValue>",
          "<frenchValue>Sommaire</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpIndex</name>",
          "<englishValue>Index</englishValue>",
          "<frenchValue>Index</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSearch</name>",
          "<englishValue>Search</englishValue>",
          "<frenchValue>Rechercher</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpAbout</name>",
          "<englishValue>About</englishValue>",
          "<frenchValue>A propos de ...</frenchValue>",
        "</term>",
        "</terms>"
      };
      StreamWriter sw = new StreamWriter(Settings.Default.LanguageFileName);
      foreach (string item in minimumVersion)
      {
        sw.WriteLine(item);
      }

      sw.Close();
    }

    private void GetWindowValue()
    {
      Width = Settings.Default.WindowWidth;
      Height = Settings.Default.WindowHeight;
      Top = Settings.Default.WindowTop < 0 ? 0 : Settings.Default.WindowTop;
      Left = Settings.Default.WindowLeft < 0 ? 0 : Settings.Default.WindowLeft;
      SetDisplayOption(Settings.Default.DisplayToolStripMenuItem);
      LoadConfigurationOptions();
    }

    private void SaveWindowValue()
    {
      Settings.Default.WindowHeight = Height;
      Settings.Default.WindowWidth = Width;
      Settings.Default.WindowLeft = Left;
      Settings.Default.WindowTop = Top;
      Settings.Default.LastLanguageUsed = frenchToolStripMenuItem.Checked ? "French" : "English";
      Settings.Default.DisplayToolStripMenuItem = GetDisplayOption();
      SaveConfigurationOptions();
      Settings.Default.Save();
    }

    private string GetDisplayOption()
    {
      if (SmallToolStripMenuItem.Checked)
      {
        return "Small";
      }

      if (MediumToolStripMenuItem.Checked)
      {
        return "Medium";
      }

      return LargeToolStripMenuItem.Checked ? "Large" : string.Empty;
    }

    private void SetDisplayOption(string option)
    {
      UncheckAllOptions();
      switch (option.ToLower())
      {
        case "small":
          SmallToolStripMenuItem.Checked = true;
          break;
        case "medium":
          MediumToolStripMenuItem.Checked = true;
          break;
        case "large":
          LargeToolStripMenuItem.Checked = true;
          break;
        default:
          SmallToolStripMenuItem.Checked = true;
          break;
      }
    }

    private void UncheckAllOptions()
    {
      SmallToolStripMenuItem.Checked = false;
      MediumToolStripMenuItem.Checked = false;
      LargeToolStripMenuItem.Checked = false;
    }

    private void FormMainFormClosing(object sender, FormClosingEventArgs e)
    {
      SaveWindowValue();
    }

    private void FrenchToolStripMenuItemClick(object sender, EventArgs e)
    {
      _currentLanguage = Language.French.ToString();
      SetLanguage(Language.French.ToString());
      AdjustAllControls();
    }

    private void EnglishToolStripMenuItemClick(object sender, EventArgs e)
    {
      _currentLanguage = Language.English.ToString();
      SetLanguage(Language.English.ToString());
      AdjustAllControls();
    }

    private void SetLanguage(string myLanguage)
    {
      switch (myLanguage)
      {
        case "English":
          frenchToolStripMenuItem.Checked = false;
          englishToolStripMenuItem.Checked = true;
          fileToolStripMenuItem.Text = LanguageDicoEn["MenuFile"];
          newToolStripMenuItem.Text = LanguageDicoEn["MenuFileNew"];
          openToolStripMenuItem.Text = LanguageDicoEn["MenuFileOpen"];
          saveToolStripMenuItem.Text = LanguageDicoEn["MenuFileSave"];
          saveasToolStripMenuItem.Text = LanguageDicoEn["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = LanguageDicoEn["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = LanguageDicoEn["MenufilePageSetup"];
          quitToolStripMenuItem.Text = LanguageDicoEn["MenufileQuit"];
          editToolStripMenuItem.Text = LanguageDicoEn["MenuEdit"];
          cancelToolStripMenuItem.Text = LanguageDicoEn["MenuEditCancel"];
          redoToolStripMenuItem.Text = LanguageDicoEn["MenuEditRedo"];
          cutToolStripMenuItem.Text = LanguageDicoEn["MenuEditCut"];
          copyToolStripMenuItem.Text = LanguageDicoEn["MenuEditCopy"];
          pasteToolStripMenuItem.Text = LanguageDicoEn["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = LanguageDicoEn["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = LanguageDicoEn["MenuTools"];
          personalizeToolStripMenuItem.Text = LanguageDicoEn["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = LanguageDicoEn["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = LanguageDicoEn["MenuLanguage"];
          englishToolStripMenuItem.Text = LanguageDicoEn["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = LanguageDicoEn["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = LanguageDicoEn["MenuHelp"];
          summaryToolStripMenuItem.Text = LanguageDicoEn["MenuHelpSummary"];
          indexToolStripMenuItem.Text = LanguageDicoEn["MenuHelpIndex"];
          searchToolStripMenuItem.Text = LanguageDicoEn["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = LanguageDicoEn["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = LanguageDicoEn["Display"];
          SmallToolStripMenuItem.Text = LanguageDicoEn["Small"];
          MediumToolStripMenuItem.Text = LanguageDicoEn["Medium"];
          LargeToolStripMenuItem.Text = LanguageDicoEn["Large"];


          _currentLanguage = "English";
          break;
        case "French":
          frenchToolStripMenuItem.Checked = true;
          englishToolStripMenuItem.Checked = false;
          fileToolStripMenuItem.Text = LanguageDicoFr["MenuFile"];
          newToolStripMenuItem.Text = LanguageDicoFr["MenuFileNew"];
          openToolStripMenuItem.Text = LanguageDicoFr["MenuFileOpen"];
          saveToolStripMenuItem.Text = LanguageDicoFr["MenuFileSave"];
          saveasToolStripMenuItem.Text = LanguageDicoFr["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = LanguageDicoFr["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = LanguageDicoFr["MenufilePageSetup"];
          quitToolStripMenuItem.Text = LanguageDicoFr["MenufileQuit"];
          editToolStripMenuItem.Text = LanguageDicoFr["MenuEdit"];
          cancelToolStripMenuItem.Text = LanguageDicoFr["MenuEditCancel"];
          redoToolStripMenuItem.Text = LanguageDicoFr["MenuEditRedo"];
          cutToolStripMenuItem.Text = LanguageDicoFr["MenuEditCut"];
          copyToolStripMenuItem.Text = LanguageDicoFr["MenuEditCopy"];
          pasteToolStripMenuItem.Text = LanguageDicoFr["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = LanguageDicoFr["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = LanguageDicoFr["MenuTools"];
          personalizeToolStripMenuItem.Text = LanguageDicoFr["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = LanguageDicoFr["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = LanguageDicoFr["MenuLanguage"];
          englishToolStripMenuItem.Text = LanguageDicoFr["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = LanguageDicoFr["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = LanguageDicoFr["MenuHelp"];
          summaryToolStripMenuItem.Text = LanguageDicoFr["MenuHelpSummary"];
          indexToolStripMenuItem.Text = LanguageDicoFr["MenuHelpIndex"];
          searchToolStripMenuItem.Text = LanguageDicoFr["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = LanguageDicoFr["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = LanguageDicoFr["Display"];
          SmallToolStripMenuItem.Text = LanguageDicoFr["Small"];
          MediumToolStripMenuItem.Text = LanguageDicoFr["Medium"];
          LargeToolStripMenuItem.Text = LanguageDicoFr["Large"];

          _currentLanguage = "French";
          break;
        default:
          SetLanguage("English");
          break;
      }
    }

    private void CutToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { textBoxTitle });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CutToClipboard(tb);
      }
    }

    private void CopyToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { textBoxTitle });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CopyToClipboard(tb);
      }
    }

    private void PasteToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { textBoxTitle });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        PasteFromClipboard(tb);
      }
    }

    private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { textBoxTitle });
      TextBox control = focusedControl as TextBox;
      control?.SelectAll();
    }

    private void CutToClipboard(TextBoxBase tb, string errorMessage = "nothing")
    {
      if (tb != ActiveControl)
      {
        return;
      }

      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIs") + Punctuation.OneSpace +
          Translate(errorMessage) + Punctuation.OneSpace +
          Translate("ToCut") + Punctuation.OneSpace, Translate(errorMessage),
          MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(errorMessage), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
      tb.SelectedText = string.Empty;
    }

    private void CopyToClipboard(TextBoxBase tb, string message = "nothing")
    {
      if (tb != ActiveControl)
      {
        return;
      }

      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIsNothingToCopy") + Punctuation.OneSpace,
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
    }

    private void PasteFromClipboard(TextBoxBase textBox)
    {
      if (textBox != ActiveControl) return;
      var selectionIndex = textBox.SelectionStart;
      textBox.SelectedText = Clipboard.GetText();
      textBox.SelectionStart = selectionIndex + Clipboard.GetText().Length;
    }

    private void DisplayMessage(string message, string title, MessageBoxButtons buttons)
    {
      MessageBox.Show(this, message, title, buttons);
    }

    private string Translate(string word)
    {
      string result = string.Empty;
      switch (_currentLanguage.ToLower())
      {
        case "english":
          result = LanguageDicoEn.ContainsKey(word) ? LanguageDicoEn[word] :
           "the term: \"" + word + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
        case "french":
          result = LanguageDicoFr.ContainsKey(word) ? LanguageDicoFr[word] :
            "the term: \"" + word + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
      }

      return result;
    }

    private static Control FindFocusedControl(Control container)
    {
      foreach (Control childControl in container.Controls.Cast<Control>().Where(childControl => childControl.Focused))
      {
        return childControl;
      }

      return (from Control childControl in container.Controls
              select FindFocusedControl(childControl)).FirstOrDefault(maybeFocusedControl => maybeFocusedControl != null);
    }

    private static Control FindFocusedControl(List<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(params Control[] container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(IEnumerable<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static string PeekDirectory()
    {
      string result = string.Empty;
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      if (fbd.ShowDialog() == DialogResult.OK)
      {
        result = fbd.SelectedPath;
      }

      return result;
    }

    private string PeekFile()
    {
      string result = string.Empty;
      OpenFileDialog fd = new OpenFileDialog();
      if (fd.ShowDialog() == DialogResult.OK)
      {
        result = fd.SafeFileName;
      }

      return result;
    }

    private void SmallToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      SmallToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void MediumToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      MediumToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void LargeToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      LargeToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private static void AdjustControls(params Control[] listOfControls)
    {
      if (listOfControls.Length == 0)
      {
        return;
      }

      int position = listOfControls[0].Width + 33; // 33 is the initial padding, can be increased
      bool isFirstControl = true;
      foreach (Control control in listOfControls)
      {
        if (isFirstControl)
        {
          isFirstControl = false;
        }
        else
        {
          control.Left = position + 10;
          position += control.Width;
        }
      }
    }

    private void AdjustAllControls()
    {
      AdjustControls();
    }

    private void OptionsToolStripMenuItemClick(object sender, EventArgs e)
    {
      FormOptions frmOptions = new FormOptions(_configurationOptions);

      if (frmOptions.ShowDialog() == DialogResult.OK)
      {
        _configurationOptions = frmOptions.ConfigurationOptions2;
      }
    }

    private static void SetButtonEnabled(Control button, params Control[] controls)
    {
      bool result = true;
      foreach (Control ctrl in controls)
      {
        if (ctrl.GetType() == typeof(TextBox))
        {
          if (((TextBox)ctrl).Text == string.Empty)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ListView))
        {
          if (((ListView)ctrl).Items.Count == 0)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ComboBox))
        {
          if (((ComboBox)ctrl).SelectedIndex == -1)
          {
            result = false;
            break;
          }
        }
      }

      button.Enabled = result;
    }

    private void ButtonAddClick(object sender, EventArgs e)
    {
      if (textBoxTitle.Text == string.Empty)
      {
        DisplayMessage("A title is necessary", "No Title", MessageBoxButtons.OK);
        return;
      }

      Event oneEvent = new Event(textBoxTitle.Text, dateTimePickerMain.Value);
      AllEvent.AddOneEvent(oneEvent);
      RefreshAllEvents();
      listBoxMain.SetSelected(GetIndexOf(textBoxTitle.Text), true);
      UpdateSubList();
      DataFileHasBeenModified = true;
    }

    private int GetIndexOf(string text)
    {
      return listBoxMain.Items.Contains(text) ? listBoxMain.Items.IndexOf(text) : 0;
    }

    private void RefreshAllEvents()
    {
      listBoxMain.Items.Clear();
      foreach (string item in AllEvent.GlobalListOfEvents.Keys.ToList())
      {
        listBoxMain.Items.Add(item);
      }
    }

    private void ListBoxMain_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!listBoxSubItems.Visible)
      {
        listBoxSubItems.Visible = true;
      }

      if (!buttonDelete.Visible)
      {
        buttonDelete.Visible = true;
      }

      UpdateSubList();
      textBoxTitle.Text = listBoxMain.SelectedItem.ToString();
      listBoxSubItems.SelectedIndex = 0;
      textBoxTimeAgo.Text = CreateTimeSentence((DateTime)listBoxSubItems.SelectedItem);
    }

    public string CreateTimeSentence(DateTime thedate)
    {
      // create a string formatted like 3 days 2 hours ago
      StringBuilder result = new StringBuilder();
      TimeSpan timeSpan = DateTime.Now - thedate;

      var totalDays = (DateTime.Now - thedate).TotalDays;
      var totalYears = Math.Truncate(totalDays / 365);
      var totalMonths = Math.Truncate((totalDays % 365) / 30);
      var remainingDays = Math.Truncate((totalDays % 365) % 30);

      if (totalYears > 0)
      {
        result.Append($"{totalYears} {Translate("year")}{Plural((int)totalYears)} ");
      }

      if (totalMonths > 0)
      {
        result.Append($"{totalMonths} {Translate("month")}{Plural((int)totalMonths, _currentLanguage)} ");
      }

      if (timeSpan.Days > 0 && result.ToString().Length != 0)
      {
        result.Append($"{remainingDays} {Translate("day")}{Plural((int)remainingDays)} ");
      }

      if (timeSpan.Days > 0 && result.ToString().Length == 0)
      {
        result.Append($"{timeSpan.Days} {Translate("day")}{Plural(timeSpan.Days)} ");
      }

      if (timeSpan.Hours > 0)
      {
        result.Append($"{timeSpan.Hours} {Translate("hour")}{Plural(timeSpan.Hours)} ");
      }

      if (timeSpan.Minutes > 0)
      {
        result.Append($"{timeSpan.Minutes} {Translate("minute")}{Plural(timeSpan.Minutes)} ");
      }

      if (timeSpan.Seconds > 0)
      {
        result.Append($"{timeSpan.Seconds} {Translate("second")}{Plural(timeSpan.Seconds)} ");
      }

      if (timeSpan.Milliseconds > 0)
      {
        result.Append($"{timeSpan.Milliseconds} {Translate("millisecond")}{Plural(timeSpan.Milliseconds)}");
      }

      return result.ToString();
    }

    private static string Plural(int number, string language = "english")
    {
      return number > 1 ? language.ToLower() == "french" ? "" : "s" : string.Empty;
    }

    private void UpdateSubList()
    {
      // we display sub items from selected one
      if (listBoxMain.SelectedIndex == -1)
      {
        return;
      }

      listBoxSubItems.Items.Clear();

      foreach (var item in AllEvent.GlobalListOfEvents[listBoxMain.SelectedItem.ToString()])
      {
        listBoxSubItems.Items.Add(item.DateOfEvent);
      }
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Are you sure you want to remove this Event?", "Confirmation", MessageBoxButtons.YesNo) !=
          DialogResult.Yes)
      {
        return;
      }

      AllEvent.GlobalListOfEvents.Remove(listBoxMain.SelectedItem.ToString());
      RefreshAllEvents();
      listBoxSubItems.Items.Clear();
      DataFileHasBeenModified = true;
    }

    private void BackupDataFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      // we zip the XML datafile
      bool backupsuccessfull = ZipFileName(Settings.Default.DataFileName);
      DisplayMessage($"The backup of the data file was {Negate(backupsuccessfull)}successfull",
        backupsuccessfull ? "Backup successfull" : "Error backup", MessageBoxButtons.OK);
    }

    private static string Negate(bool yesOrNot)
    {
      return yesOrNot ? string.Empty : "not ";
    }

    private bool ZipFileName(string fileName)
    {
      bool result = false;
      try
      {
        using (ZipFile zip = new ZipFile())
        {
          zip.AddFile(fileName);
          zip.Save($"{fileName}.zip");
        }

        result = true;
      }
      catch (Exception exception)
      {
        DisplayMessage($"Error while zipping the file {fileName}{Environment.NewLine}The exception is {exception.Message}", "Error while zipping", MessageBoxButtons.OK);
        result = false;
      }

      return result;
    }

    private void OpenDataFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      Process task = new Process
      {
        StartInfo =
        {
          UseShellExecute = true,
          FileName = "Explorer.exe",
          Arguments =  workingDirectory,
          CreateNoWindow = false
        }
      };

      task.Start();
    }
    
    public static bool SendMail(string message, string hostServer, string userName, string password)
    {
      bool result = false;
      MailMessage mailMessage = new MailMessage {From = new MailAddress("sender@isp.fr") };
      mailMessage.To.Add(new MailAddress("addressee@isp.fr"));
      mailMessage.Subject = "Password Recover";
      mailMessage.Body = message;

      SmtpClient client = new SmtpClient
      {
        Host = hostServer, // "smtp.isp.fr",
        Port = 25,
        Timeout = 10000,
        UseDefaultCredentials = false,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        EnableSsl = true,
        Credentials = new NetworkCredential(userName, password)
    };
      try
      {
        client.Send(mailMessage);
        result = true;
      }
      catch (Exception)
      {
        result = false;
      }

      return result;
    }

    private void EmailDataFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (SendMail("test message", "smtp.isp.fr", "username", "password"))
      {
        DisplayMessage("the mail was sent correctly", "mail ok", MessageBoxButtons.OK);
      }
    }
  }
}