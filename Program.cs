using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

public interface IActivity
{
    int Index { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
}

[Serializable]
public abstract class BasePlan<T> where T : IActivity
{
    public string Title { get; set; }
    public List<T> Activities { get; set; }

    public BasePlan()
    {
        Activities = new List<T>();
    }
}

[Serializable]
public class Activity : IActivity
{
    public int Index { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

[Serializable]
public class Plan : BasePlan<IActivity>
{
    public new string Title { get; set; }
}

public class Statistics
{
    public int GetNumberOfActivities(BasePlan<IActivity> plan)
    {
        return plan.Activities.Count;
    }

    public double GetAverageDuration(BasePlan<IActivity> plan)
    {
        if (plan.Activities.Count == 0) return 0;

        double totalDuration = plan.Activities.Sum(activity => (activity.EndTime - activity.StartTime).TotalMinutes);
        return totalDuration / plan.Activities.Count;
    }
}

public class PlannerApp
{
    private List<BasePlan<IActivity>> plans = new List<BasePlan<IActivity>>();
    private IFormatter formatter = new BinaryFormatter();

    public void CreatePlan(string title)
    {
        var plan = new Plan { Title = title };
        plans.Add(plan);

        var activity = new Activity
        {
            Index = 1,
            Name = "Default Activity",
            Description = "Default Description",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };
        AddActivity(plan, activity);
    }

    public void AddActivity<T>(BasePlan<T> plan, T activity) where T : IActivity
    {
        activity.Index = plan.Activities.Count + 1;
        plan.Activities.Add(activity);
    }

    public void RemoveActivity(BasePlan<IActivity> plan, IActivity activity)
    {
        plan.Activities.Remove(activity);
        UpdateActivityIndexes(plan);
    }

    private void UpdateActivityIndexes(BasePlan<IActivity> plan)
    {
        for (int i = 0; i < plan.Activities.Count; i++)
        {
            plan.Activities[i].Index = i + 1;
        }
    }

    public List<BasePlan<IActivity>> GetDailyPlans()
    {
        return plans;
    }

    public void SavePlansToFile(string fileName)
    {
        using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            formatter.Serialize(stream, plans);
        }
    }

    public void LoadPlansFromFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                plans = (List<BasePlan<IActivity>>)formatter.Deserialize(stream);
            }
        }
    }
}

public class WinFormsUI : Form
{
    private PlannerApp app;
    private Statistics stats;
    private const string fileName = "plans.dat";
    private ListBox planListBox;
    private DataGridView activityDataGridView;
    private TextBox activityNameTextBox;
    private TextBox activityDescriptionTextBox;
    private Button createPlanButton;
    private Button addActivityButton;
    private Button removeActivityButton;
    private Button showStatsButton;
    private Button saveToFileButton;
    private Button loadFromFileButton;
    private DateTimePicker startTimePicker;
    private DateTimePicker endTimePicker;

    public WinFormsUI()
    {
        app = new PlannerApp();
        stats = new Statistics();
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Planner App";
        this.Size = new System.Drawing.Size(800, 400);

        planListBox = new ListBox();
        planListBox.Dock = DockStyle.Left;
        planListBox.Width = 200;
        planListBox.SelectedIndexChanged += PlanListBox_SelectedIndexChanged;

        activityDataGridView = new DataGridView();
        activityDataGridView.Dock = DockStyle.Fill;
        activityDataGridView.AutoGenerateColumns = true;
        activityDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        activityDataGridView.AllowUserToAddRows = false;
        activityDataGridView.AllowUserToDeleteRows = false;
        activityDataGridView.ReadOnly = true;
        activityDataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;

        this.Controls.Add(activityDataGridView);

        activityNameTextBox = new TextBox();
        activityNameTextBox.Dock = DockStyle.Top;
        activityNameTextBox.PlaceholderText = "Activity or Plan Name";
        activityNameTextBox.Margin = new Padding(10, 10, 10, 0);

        activityDescriptionTextBox = new TextBox();
        activityDescriptionTextBox.Dock = DockStyle.Top;
        activityDescriptionTextBox.PlaceholderText = "Activity Description";
        activityDescriptionTextBox.Margin = new Padding(10, 0, 10, 0);

        createPlanButton = new Button();
        createPlanButton.Text = "Create Plan";
        createPlanButton.Dock = DockStyle.Left;
        createPlanButton.Click += CreatePlanButton_Click;
        createPlanButton.Margin = new Padding(10, 0, 10, 0);

        addActivityButton = new Button();
        addActivityButton.Text = "Add Activity";
        addActivityButton.Dock = DockStyle.Right;
        addActivityButton.Click += AddActivityButton_Click;
        addActivityButton.Margin = new Padding(10, 0, 10, 0);

        removeActivityButton = new Button();
        removeActivityButton.Text = "Remove Activity";
        removeActivityButton.Dock = DockStyle.Top;
        removeActivityButton.Click += RemoveActivityButton_Click;
        removeActivityButton.Margin = new Padding(10, 0, 10, 0);

        showStatsButton = new Button();
        showStatsButton.Text = "Show Stats";
        showStatsButton.Dock = DockStyle.Top;
        showStatsButton.Click += ShowStatsButton_Click;
        showStatsButton.Margin = new Padding(10, 0, 10, 0);

        saveToFileButton = new Button();
        saveToFileButton.Text = "Save to File";
        saveToFileButton.Dock = DockStyle.Top;
        saveToFileButton.Click += SaveToFileButton_Click;
        saveToFileButton.Margin = new Padding(10, 0, 10, 0);

        loadFromFileButton = new Button();
        loadFromFileButton.Text = "Load from File";
        loadFromFileButton.Dock = DockStyle.Top;
        loadFromFileButton.Click += LoadFromFileButton_Click;
        loadFromFileButton.Margin = new Padding(10, 0, 10, 0);

        startTimePicker = new DateTimePicker();
        startTimePicker.Format = DateTimePickerFormat.Custom;
        startTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
        startTimePicker.Dock = DockStyle.Top;
        startTimePicker.Margin = new Padding(10, 0, 10, 0);
        this.Controls.Add(startTimePicker);

        endTimePicker = new DateTimePicker();
        endTimePicker.Format = DateTimePickerFormat.Custom;
        endTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
        endTimePicker.Dock = DockStyle.Top;
        endTimePicker.Margin = new Padding(10, 0, 10, 0);
        this.Controls.Add(endTimePicker);

        TableLayoutPanel buttonPanel = new TableLayoutPanel();
        buttonPanel.Dock = DockStyle.Bottom;
        buttonPanel.Height = 100;
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        buttonPanel.Controls.Add(createPlanButton, 0, 0);
        buttonPanel.Controls.Add(addActivityButton, 1, 0);
        buttonPanel.Controls.Add(removeActivityButton, 0, 1);
        buttonPanel.Controls.Add(showStatsButton, 1, 1);
        buttonPanel.Controls.Add(saveToFileButton, 0, 2);
        buttonPanel.Controls.Add(loadFromFileButton, 1, 2);

        this.Controls.Add(planListBox);
        this.Controls.Add(activityNameTextBox);
        this.Controls.Add(activityDescriptionTextBox);
        this.Controls.Add(buttonPanel);
    }

    private void PlanListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (planListBox.SelectedIndex >= 0 && planListBox.SelectedIndex < app.GetDailyPlans().Count)
        {
            var selectedPlan = app.GetDailyPlans()[planListBox.SelectedIndex];
            activityDataGridView.DataSource = selectedPlan.Activities;
        }
    }

    private void CreatePlanButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(activityNameTextBox.Text))
        {
            app.CreatePlan(activityNameTextBox.Text);
            RefreshPlanListBox();
        }
    }

    private void AddActivityButton_Click(object sender, EventArgs e)
    {
        if (planListBox.SelectedItem != null && !string.IsNullOrEmpty(activityNameTextBox.Text))
        {
            var selectedPlan = (BasePlan<IActivity>)planListBox.SelectedItem;
            DateTime startTime = startTimePicker.Value;
            DateTime endTime = endTimePicker.Value;

            if (startTime < endTime)
            {
                var activity = new Activity
                {
                    Name = activityNameTextBox.Text,
                    Description = activityDescriptionTextBox.Text,
                    StartTime = startTime,
                    EndTime = endTime
                };
                app.AddActivity(selectedPlan, activity);
                RefreshActivityDataGridView(selectedPlan);
            }
            else
            {
                MessageBox.Show("Start time must be before end time.", "Error");
            }
        }
    }

    private void RemoveActivityButton_Click(object sender, EventArgs e)
    {
        if (planListBox.SelectedItem != null && planListBox.SelectedIndex >= 0)
        {
            int selectedIndex = planListBox.SelectedIndex;

            if (selectedIndex < app.GetDailyPlans().Count)
            {
                var selectedPlan = app.GetDailyPlans()[selectedIndex];

                if (activityDataGridView.SelectedRows.Count > 0)
                {
                    var selectedActivity = (IActivity)activityDataGridView.SelectedRows[0].DataBoundItem;

                    app.RemoveActivity(selectedPlan, selectedActivity);
                    RefreshActivityDataGridView(selectedPlan);
                }
                else
                {
                    MessageBox.Show("Please select an activity to remove.", "Error");
                }
            }
        }
    }

    private void ShowStatsButton_Click(object sender, EventArgs e)
    {
        if (planListBox.SelectedItem != null)
        {
            var selectedPlan = (BasePlan<IActivity>)planListBox.SelectedItem;
            var statsMessage = $"Number of Activities: {stats.GetNumberOfActivities(selectedPlan)}\n" +
                               $"Average Duration: {stats.GetAverageDuration(selectedPlan):F2} minutes";
            MessageBox.Show(statsMessage, "Statistics");
        }
    }

    private void SaveToFileButton_Click(object sender, EventArgs e)
    {
        app.SavePlansToFile(fileName);
    }

    private void LoadFromFileButton_Click(object sender, EventArgs e)
    {
        app.LoadPlansFromFile(fileName);
        RefreshPlanListBox();
    }

    private void RefreshPlanListBox()
    {
        planListBox.Items.Clear();
        foreach (var plan in app.GetDailyPlans())
        {
            planListBox.Items.Add(plan);
            planListBox.DisplayMember = "Title";
        }
    }

    private void RefreshActivityDataGridView(BasePlan<IActivity> plan)
    {
        activityDataGridView.DataSource = null;
        activityDataGridView.DataSource = plan.Activities;
        activityDataGridView.Refresh();
        this.Refresh();
    }

    [STAThread]
    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        WinFormsUI mainForm = new WinFormsUI();
        try
        {
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
