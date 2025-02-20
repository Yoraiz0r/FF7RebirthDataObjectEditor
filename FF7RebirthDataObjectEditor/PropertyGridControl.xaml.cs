using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FF7R2.DataObject;
using FF7RebirthDataObjectEditor.FF7Types;

namespace FF7RebirthDataObjectEditor;

public partial class PropertyGridControl : UserControl
{
    private SolidColorBrush transparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
    private Style? buttonStyle;

    public ObservableCollection<EntryRow> AssetEntries
	{
		get { return (ObservableCollection<EntryRow>)GetValue(AssetEntriesProperty); }
		set { SetValue(AssetEntriesProperty, value); }
	}

	public static readonly DependencyProperty AssetEntriesProperty =
		DependencyProperty.Register(nameof(AssetEntries), typeof(ObservableCollection<EntryRow>), typeof(PropertyGridControl), new PropertyMetadata(null));

	public PropertyGridControl()
	{
		InitializeComponent();
        Style? baseStyle = Application.Current.TryFindResource(typeof(Button)) as Style;
        if (baseStyle == null)
            return;
        buttonStyle = new Style()
        {
            BasedOn = baseStyle,
            TargetType = typeof(Button)
        };
        DataTrigger rowSelectedTrigger = new DataTrigger();
        var binding = new Binding("IsSelected");
        binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow),1);
        rowSelectedTrigger.Binding = binding;
        rowSelectedTrigger.Value = true;
        var dynamicResource = new DynamicResourceExtension("DataGridRowSelectedForegroundThemeBrush");
        rowSelectedTrigger.Setters.Add(new Setter(Button.ForegroundProperty, dynamicResource));
        buttonStyle.Triggers.Add(rowSelectedTrigger);
    }
	
    public void GenerateColumns(bool withEntryColumn)
    {
        assetDataGrid.Columns.Clear();

        assetDataGrid.Columns.Add(new System.Windows.Controls.DataGridTextColumn
        {
            Header = "#",
            Binding = new Binding("Index"),
            IsReadOnly = true,
        });
        if (withEntryColumn)
            assetDataGrid.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            {
                Header = "Entry",
                Binding = new Binding("Name")
            });

        if (AssetEntries.Count <= 0)
            return;
            
        
        var firstEntry = AssetEntries.First();
        var properties = firstEntry.Data.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            if (Utils.IsComplexProperty(properties[i]))
            {
                var buttonFactory = new FrameworkElementFactory(typeof(Button));
                buttonFactory.SetBinding(Button.ContentProperty, new Binding($"Data.Properties[{i}].Value"));
                buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click));
                buttonFactory.SetBinding(Button.TagProperty, new Binding($"Data.Properties[{i}]"));
                // buttonFactory.SetBinding( Button.DataContextProperty, new Binding($"Data.Properties[{0}]"));
                buttonFactory.SetValue(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
                buttonFactory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                buttonFactory.SetValue(Button.BackgroundProperty, transparentBrush);

                if(buttonStyle != null)
                    buttonFactory.SetValue(Button.StyleProperty, buttonStyle);
                assetDataGrid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = properties[i].Name,
                    CellTemplate = new DataTemplate
                    {
                        VisualTree = buttonFactory
                    }
                });
            }
            else
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = properties[i].Name,
                    Binding = new Binding($"Data.Properties[{i}].Value")
                };
                assetDataGrid.Columns.Add(textColumn);
            }
        } 
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var clickedButton = sender as Button;
        if (clickedButton == null)
            return;

        var property = clickedButton.Tag as ArrayPropertyViewModel;
        if (property == null)
            return;

        var panel = new ArrayPropertiesWindow(property);

        
        panel.Show();
        panel.PropertyGrid.GenerateColumns(false);
        if (clickedButton.DataContext is EntryRow entryRow)
            panel.Title = $"{entryRow.Name} | {property.Name} Editor";
    }
    
}