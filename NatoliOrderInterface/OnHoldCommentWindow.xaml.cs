using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Colors = System.Windows.Media.Colors;
using System.Data.SqlClient;
using System.Timers;
using NatoliOrderInterface.Models.DriveWorks;
using System.Threading;
using System.Collections.ObjectModel;
using NatoliOrderInterface.Models.Projects;
using System.IO;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OnHoldCommentWindow : Window
    {
        private string projectType;
        private string projectNumber;
        private string revisionNumber;
        private readonly MainWindow parent;
        private User User;
        private readonly bool engineeringProject = false;
        private readonly ProjectWindow projectWindow = null;
        public OnHoldCommentWindow(string projectType, int projectNumber, int? revisionNumber, MainWindow parent, User User, bool engineeringProject = false, ProjectWindow projectWindow = null)
        {
            InitializeComponent();
            this.projectType = projectType;
            this.projectNumber = projectNumber.ToString();
            this.revisionNumber = revisionNumber is null ? "0" : revisionNumber.ToString();
            this.parent = parent;
            this.User = User;
            this.Top = parent.Top;
            this.Left = parent.Left;
            this.engineeringProject = engineeringProject;
            this.projectWindow = projectWindow;
            //this.Show();
        }

        #region Functions
        private void SetOnHold()
        {
            try
            {
                using var _nat02Context = new NAT02Context();
                if (!engineeringProject)
                {
                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();
                    

                    if (_projectsContext.HoldStatus.Any(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revisionNumber))
                    {
                        // Update data in HoldStatus
                        HoldStatus holdStatus = _projectsContext.HoldStatus.Where(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revisionNumber).First();
                        holdStatus.HoldStatus1 = "ON HOLD";
                        holdStatus.TimeSubmitted = DateTime.Now;
                        holdStatus.OnHoldComment = CommentBox.Text;
                        holdStatus.User = User.GetUserName() ?? Environment.UserName.ToLower();
                        _projectsContext.HoldStatus.Update(holdStatus);
                    }
                    else
                    {
                        // Insert into HoldStatus
                        HoldStatus holdStatus = new HoldStatus();
                        holdStatus.ProjectNumber = projectNumber;
                        holdStatus.RevisionNumber = revisionNumber;
                        holdStatus.TimeSubmitted = DateTime.Now;
                        holdStatus.HoldStatus1 = "ON HOLD";
                        holdStatus.OnHoldComment = CommentBox.Text;
                        holdStatus.User = User.GetUserName() ?? Environment.UserName.ToLower();
                        _projectsContext.HoldStatus.Add(holdStatus);
                    }

                    // Drive specification transition name to "On Hold - " projectType
                    string _name = projectNumber + (Convert.ToInt32(revisionNumber) > 0 ? "_" + revisionNumber : "");
                    Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                    spec.StateName = "On Hold - " + projectType;
                    _driveworksContext.Specifications.Update(spec);

                    var name = IMethods.GetDisplayNameFromDWPrincipalID(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber.ToString() == projectNumber && p.RevisionNumber.ToString() == revisionNumber).Csr).Split(' ');
                    EoiNotificationsActive eoiNotificationsActive = new EoiNotificationsActive()
                    {
                        Type = "Project",
                        Number = projectNumber,
                        Message = "Project has been put on hold.",
                        User = (name[0].Substring(0, 1) + name[1]).ToLower(),
                        Timestamp = DateTime.Now
                    };
                    _nat02Context.EoiNotificationsActive.Add(eoiNotificationsActive);
                    if (!string.IsNullOrEmpty(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber.ToString() == projectNumber && p.RevisionNumber.ToString() == revisionNumber).ReturnToCsr))
                    {
                        name = IMethods.GetDisplayNameFromDWPrincipalID(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber.ToString() == projectNumber && p.RevisionNumber.ToString() == revisionNumber).ReturnToCsr).Split(' ');
                        EoiNotificationsActive eoiNotificationsActive1 = new EoiNotificationsActive()
                        {
                            Type = "Project",
                            Number = projectNumber,
                            Message = "Project has been put on hold.",
                            User = (name[0].Substring(0, 1) + name[1]).ToLower(),
                            Timestamp = DateTime.Now

                        };
                        _nat02Context.EoiNotificationsActive.Add(eoiNotificationsActive1);
                    }


                    _projectsContext.SaveChanges();
                    _driveworksContext.SaveChanges();
                    _nat02Context.SaveChanges();
                    _projectsContext.Dispose();
                    _driveworksContext.Dispose();
                    _nat02Context.Dispose();
                    parent.BoolValue = true;
                }
                else
                {
                    using var _projectsContext = new ProjectsContext();

                    EngineeringProjects eProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber.ToString() && p.RevNumber == revisionNumber.ToString());
                    eProject.OnHold = true;
                    eProject.OnHoldComment = CommentBox.Text;
                    eProject.OnHoldDateTime = DateTime.UtcNow;
                    eProject.OnHoldUser = User.GetUserName() ?? Environment.UserName.ToLower();
                    var name = IMethods.GetDisplayNameFromDWPrincipalID(eProject.CSR).Split(' ');
                    EoiNotificationsActive eoiNotificationsActive = new EoiNotificationsActive()
                    {
                        Type = "Project",
                        Number = projectNumber,
                        Message = "Project has been put on hold.",
                        User = (name[0].Substring(0,1)+name[1]).ToLower(),
                        Timestamp = DateTime.Now
                    };
                    _nat02Context.EoiNotificationsActive.Add(eoiNotificationsActive);
                    if(!string.IsNullOrEmpty(eProject.ReturnToCSR) && eProject.ReturnToCSR.Length>0)
                    {
                        name = IMethods.GetDisplayNameFromDWPrincipalID(eProject.ReturnToCSR).Split(' ');
                        EoiNotificationsActive eoiNotificationsActive1 = new EoiNotificationsActive()
                        {
                            Type = "Project",
                            Number = projectNumber,
                            Message = "Project has been put on hold.",
                            User = (name[0].Substring(0, 1) + name[1]).ToLower(),
                            Timestamp = DateTime.Now
                        };
                        _nat02Context.EoiNotificationsActive.Add(eoiNotificationsActive1);
                    }


                    //string _name = projectNumber + (Convert.ToInt32(revisionNumber) > 0 ? "_" + revisionNumber : "");
                    //Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                    //spec.StateName = "On Hold - " + projectType;
                    //_driveworksContext.Specifications.Update(spec);

                    _projectsContext.SaveChanges();
                    _nat02Context.SaveChanges();
                    _projectsContext.Dispose();
                    _nat02Context.Dispose();
                    if (projectWindow != null)
                    {
                        projectWindow.PutOnHoldButton.Content = "Take Off Hold";
                    }
                    
                }
                string path = @"R:\TOOLING AUTOMATION\Project Specifications\" + projectNumber + "\\" ;
                if (File.Exists(path + "On_Hold_Comment.txt"))
                {
                    int i = 1;
                    while (File.Exists(path + "On_Hold_Comment_" + i + ".txt"))
                    {
                        i++;
                    }
                    path = path + "On_Hold_Comment_" + i + ".txt";
                }

                try
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.Write("Comment: " + Environment.NewLine +
                            CommentBox.Text + Environment.NewLine + Environment.NewLine +
                            "Time: " + Convert.ToString(DateTime.Now) + Environment.NewLine +
                            "User: " + User.GetUserName());
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WriteToErrorLog("SetOnHold", ex.Message);
            }
        }
        #endregion

        #region ErrorHandling
        //private void WriteToErrorLog(string errorLoc, string errorMessage)
        //{
        //    string path = @"\\nshare\vb_apps\NatoliOrderInterface\Logs\Error_Log.txt";
        //    System.IO.StreamReader sr = new System.IO.StreamReader(path);
        //    string existing = sr.ReadToEnd();
        //    existing = existing.TrimEnd();
        //    sr.Close();
        //    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
        //    sw.Write(DateTime.Now + "  " + this.User.GetUserName() + "  " + errorLoc + "\r\n" + errorMessage.PadLeft(20) + "\r\n" + existing);
        //    sw.Flush();
        //    sw.Close();
        //}
        #endregion

        #region Events
        private void CommentBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (CommentBox.Text.Length > 249)
                {
                    CommentBox.Text = CommentBox.Text.Remove(250);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WriteToErrorLog("CommentBox_PreviewKeyUp", ex.Message);
            }
        }

        private void PutOnHoldButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CommentBox.Text.Length > 0)
                {
                    SetOnHold();
                    if (projectWindow != null)
                    {
                        Dispatcher.Invoke(() => projectWindow.PutOnHoldButton.Content = "Take Off Hold");
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show("You must have a comment.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WriteToErrorLog("PutOnHoldButton_Click", ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WriteToErrorLog("CancelButton_Click", ex.Message);
            }
        }
        #endregion
    }

}

