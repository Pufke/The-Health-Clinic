﻿using Controller.MedicalRecordContr;
using Controller.PatientContr;
using Model.MedicalRecord;
using Model.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SekretarWPF
{
    /// <summary>
    /// Interaction logic for UserControlPatients.xaml
    /// </summary>
    public partial class UserControlPatients : UserControl
    {
        PatientController controller = new PatientController();

        public static ObservableCollection<PatientModel> data = new ObservableCollection<PatientModel>();

        AddMedicalRecord addMedicalRecord;
        private CollectionView view;

        SortDescription dateSort = new SortDescription("DateOfBirth", ListSortDirection.Descending);
        SortDescription nameSort = new SortDescription("Name", ListSortDirection.Ascending);

        public UserControlPatients()
        {
            InitializeComponent();
            if(data.Count == 0)
                foreach(PatientModel pm in controller.GetAllPatients())
                {
                    data.Add(pm);
                }
            this.lvMedicalRecords.ItemsSource = data;
            addMedicalRecord = new AddMedicalRecord();
            var windows = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            addMedicalRecord.Owner = windows;

            view = (CollectionView)CollectionViewSource.GetDefaultView(lvMedicalRecords.ItemsSource);
            view.Filter = MedicalRecordFilter;
        }

        private void buttonDodaj_Click(object sender, RoutedEventArgs e)
        {
            addMedicalRecord.AddNewMedicalRecord();
        }

        private bool MedicalRecordFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
            {
                this.bRemoveSearch.Visibility = Visibility.Hidden;
                return true;
            }
            else
            {
                this.bRemoveSearch.Visibility = Visibility.Visible;
                string fulname = (item as PatientModel).Name + (item as PatientModel).Surname;
                return (fulname.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lvMedicalRecords.ItemsSource).Refresh();
        }


        private void lvMedicalRecords_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PatientModel patientModel = ((FrameworkElement)e.OriginalSource).DataContext as PatientModel;
            addMedicalRecord.EditMedicalRecord(patientModel);
        }

        private ListViewItem _currentItem = null;
        private PatientModel _currentMRecord = null;
        private void ListViewItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var item = sender as ListViewItem;
            if (!Equals(_currentItem, item))
            {                    
                _currentItem = item;
                _currentMRecord = item.DataContext as PatientModel;
                getIconsReference(_currentItem).Visibility = Visibility.Visible;
            }
        }

        private void ListViewItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var item = sender as ListViewItem;
            if(_currentItem.Equals(item))
            {
                getIconsReference(_currentItem).Visibility = Visibility.Hidden;
                _currentItem = null;
            } 
        }

        private StackPanel getIconsReference(ListViewItem item)
        {
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            StackPanel icons = (StackPanel)myDataTemplate.FindName("Icons", myContentPresenter);
            return icons;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void bEdit_Click(object sender, RoutedEventArgs e)
        {
            bool? Result = new MessageBoxCustom("Are you sure you want to delete " + _currentMRecord.Name + "'s user account?", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();

            if (Result.Value)
            {
                controller.deletePatientUserAccount(_currentMRecord);
            }
        }

        private void bDelete_Click(object sender, RoutedEventArgs e)
        {
            bool? Result = new MessageBoxCustom("Are you sure you want to delete " + _currentMRecord.Name + "'s medical record?", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();

            if (Result.Value)
            {
                controller.deletePatient(_currentMRecord);
                data.Remove(_currentMRecord);
            }

        }

        private void bRemoveSearch_Click(object sender, RoutedEventArgs e)
        {
            this.txtFilter.Text = null;
        }

        private void CheckBox_Checked_DateSort(object sender, RoutedEventArgs e)
        {
            view.SortDescriptions.Add(dateSort);
        }

        private void CheckBox_Unchecked_DateSort(object sender, RoutedEventArgs e)
        {
            view.SortDescriptions.Remove(dateSort);
        }

        private void CheckBox_Checked_NameSort(object sender, RoutedEventArgs e)
        {
            view.SortDescriptions.Add(nameSort);
        }

        private void CheckBox_Unchecked_NameSort(object sender, RoutedEventArgs e)
        {
            view.SortDescriptions.Remove(nameSort);
        }

        private List<PatientModel> selectedMedicalRecords = new List<PatientModel>();

        private void lvMedicalRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<PatientModel> currentSelection = new List<PatientModel>();
            for (int i = 0; i < lvMedicalRecords.SelectedItems.Count; i++)
            {
                PatientModel selectedMedicalRecord = (PatientModel)this.lvMedicalRecords.SelectedItems[i];
                currentSelection.Add(selectedMedicalRecord);
                if (!selectedMedicalRecords.Contains(selectedMedicalRecord))
                {
                    selectedMedicalRecords.Add(selectedMedicalRecord);
                }
            }

            List<PatientModel> toRemove = new List<PatientModel>();
            foreach(PatientModel medicalRecord in selectedMedicalRecords)
            {
                if(!currentSelection.Contains(medicalRecord))
                {
                    toRemove.Add(medicalRecord);
                }
            }
            foreach(PatientModel medicalRecord in toRemove)
            {
                selectedMedicalRecords.Remove(medicalRecord);
            }

            if(this.lvMedicalRecords.SelectedItems.Count > 0)
            {
                this.bDeleteSelection.Visibility = Visibility.Visible;
            } else
            {
                this.bDeleteSelection.Visibility = Visibility.Hidden;
            }

        }

        private void bDeleteSelection_Click(object sender, RoutedEventArgs e)
        {
            bool? Result = new MessageBoxCustom("Are you sure you want to delete selected medical records?", MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();

            if (Result.Value)
            {
                List<PatientModel> toDelete = new List<PatientModel>(selectedMedicalRecords);

                foreach (PatientModel medicalRecord in toDelete)
                {
                    controller.deletePatient(medicalRecord);
                    data.Remove(medicalRecord);
                }
            }
        }
    }
}