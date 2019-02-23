﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using mRemoteNG.App;
using mRemoteNG.App.Info;
using mRemoteNG.App.Update;
using mRemoteNG.Security.SymmetricEncryption;
using mRemoteNG.Tools;
using mRemoteNG.UI.TaskDialog;

namespace mRemoteNG.UI.Forms.OptionsPages
{
    public sealed partial class UpdatesPage
    {
        #region Private Fields

        private AppUpdater _appUpdate;

        #endregion

        public UpdatesPage()
        {
            InitializeComponent();
            ApplyTheme();
            PageIcon = Resources.Update_Icon;
        }

        #region Public Methods

        public override string PageName
        {
            get => Language.TabUpdates;
            set { }
        }

        public override void ApplyLanguage()
        {
            base.ApplyLanguage();

            lblUpdatesExplanation.Text = Language.UpdateCheck;

            chkCheckForUpdatesOnStartup.Text = Language.CheckForUpdatesOnStartup;
            btnUpdateCheckNow.Text = Language.CheckNow;

            groupBoxReleaseChannel.Text = Language.ReleaseChannel;
            lblReleaseChannelExplanation.Text = Language.ReleaseChannelExplanation;

            chkUseProxyForAutomaticUpdates.Text = Language.CheckboxUpdateUseProxy;
            lblProxyAddress.Text = $@"{Language.LabelAddress}:";
            lblProxyPort.Text = $@"{Language.PropertyNamePort}:";

            chkUseProxyAuthentication.Text = Language.CheckboxProxyAuthentication;
            lblProxyUsername.Text = $@"{Language.PropertyNameUsername}:";
            lblProxyPassword.Text = $@"{Language.PropertyNamePassword}:";

            btnTestProxy.Text = Language.ButtonTestProxy;
        }

        public override void LoadSettings()
        {
            chkCheckForUpdatesOnStartup.Checked = Settings.Default.CheckForUpdatesOnStartup;
            cboUpdateCheckFrequency.Enabled = chkCheckForUpdatesOnStartup.Checked;
            cboUpdateCheckFrequency.Items.Clear();
            var nDaily = cboUpdateCheckFrequency.Items.Add(Language.UpdateFrequencyDaily);
            var nWeekly = cboUpdateCheckFrequency.Items.Add(Language.UpdateFrequencyWeekly);
            var nMonthly = cboUpdateCheckFrequency.Items.Add(Language.UpdateFrequencyMonthly);
            if (Settings.Default.CheckForUpdatesFrequencyDays < 1)
            {
                chkCheckForUpdatesOnStartup.Checked = false;
                cboUpdateCheckFrequency.SelectedIndex = nDaily;
            } // Daily
            else
                switch (Settings.Default.CheckForUpdatesFrequencyDays)
                {
                    case 1:
                        cboUpdateCheckFrequency.SelectedIndex = nDaily;
                        break;
                    case 7:
                        cboUpdateCheckFrequency.SelectedIndex = nWeekly;
                        break;
                    case 31:
                        cboUpdateCheckFrequency.SelectedIndex = nMonthly;
                        break;
                    default:
                        var nCustom =
                            cboUpdateCheckFrequency.Items.Add(string.Format(Language.UpdateFrequencyCustom,
                                                                            Settings
                                                                                .Default.CheckForUpdatesFrequencyDays));
                        cboUpdateCheckFrequency.SelectedIndex = nCustom;
                        break;
                }

            var stable = cboReleaseChannel.Items.Add(UpdateChannelInfo.STABLE);
            var beta = cboReleaseChannel.Items.Add(UpdateChannelInfo.BETA);
            var dev = cboReleaseChannel.Items.Add(UpdateChannelInfo.DEV);
            switch (Settings.Default.UpdateChannel)
            {
                case UpdateChannelInfo.STABLE:
                    cboReleaseChannel.SelectedIndex = stable;
                    break;
                case UpdateChannelInfo.BETA:
                    cboReleaseChannel.SelectedIndex = beta;
                    break;
                case UpdateChannelInfo.DEV:
                    cboReleaseChannel.SelectedIndex = dev;
                    break;
                default:
                    cboReleaseChannel.SelectedIndex = stable;
                    break;
            }

            chkUseProxyForAutomaticUpdates.Checked = Settings.Default.UpdateUseProxy;
            pnlProxyBasic.Enabled = Settings.Default.UpdateUseProxy;
            txtProxyAddress.Text = Settings.Default.UpdateProxyAddress;
            numProxyPort.Value = Convert.ToDecimal(Settings.Default.UpdateProxyPort);

            chkUseProxyAuthentication.Checked = Settings.Default.UpdateProxyUseAuthentication;
            pnlProxyAuthentication.Enabled = Settings.Default.UpdateProxyUseAuthentication;
            txtProxyUsername.Text = Settings.Default.UpdateProxyAuthUser;
            var cryptographyProvider = new LegacyRijndaelCryptographyProvider();
            txtProxyPassword.Text =
                cryptographyProvider.Decrypt(Settings.Default.UpdateProxyAuthPass, Runtime.EncryptionKey);

            btnTestProxy.Enabled = Settings.Default.UpdateUseProxy;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            Settings.Default.CheckForUpdatesOnStartup = chkCheckForUpdatesOnStartup.Checked;
            if (cboUpdateCheckFrequency.SelectedItem.ToString() == Language.UpdateFrequencyDaily)
            {
                Settings.Default.CheckForUpdatesFrequencyDays = 1;
            }
            else if (cboUpdateCheckFrequency.SelectedItem.ToString() == Language.UpdateFrequencyWeekly)
            {
                Settings.Default.CheckForUpdatesFrequencyDays = 7;
            }
            else if (cboUpdateCheckFrequency.SelectedItem.ToString() == Language.UpdateFrequencyMonthly)
            {
                Settings.Default.CheckForUpdatesFrequencyDays = 31;
            }

            Settings.Default.UpdateChannel = cboReleaseChannel.Text;

            Settings.Default.UpdateUseProxy = chkUseProxyForAutomaticUpdates.Checked;
            Settings.Default.UpdateProxyAddress = txtProxyAddress.Text;
            Settings.Default.UpdateProxyPort = (int)numProxyPort.Value;

            Settings.Default.UpdateProxyUseAuthentication = chkUseProxyAuthentication.Checked;
            Settings.Default.UpdateProxyAuthUser = txtProxyUsername.Text;
            var cryptographyProvider = new LegacyRijndaelCryptographyProvider();
            Settings.Default.UpdateProxyAuthPass =
                cryptographyProvider.Encrypt(txtProxyPassword.Text, Runtime.EncryptionKey);
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void chkCheckForUpdatesOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            cboUpdateCheckFrequency.Enabled = chkCheckForUpdatesOnStartup.Checked;
        }

        private void btnUpdateCheckNow_Click(object sender, EventArgs e)
        {
            Windows.Show(WindowType.Update);
        }

        private void chkUseProxyForAutomaticUpdates_CheckedChanged(object sender, EventArgs e)
        {
            pnlProxyBasic.Enabled = chkUseProxyForAutomaticUpdates.Checked;
            btnTestProxy.Enabled = chkUseProxyForAutomaticUpdates.Checked;

            if (chkUseProxyForAutomaticUpdates.Checked)
            {
                chkUseProxyAuthentication.Enabled = true;

                if (chkUseProxyAuthentication.Checked)
                {
                    pnlProxyAuthentication.Enabled = true;
                }
            }
            else
            {
                chkUseProxyAuthentication.Enabled = false;
                pnlProxyAuthentication.Enabled = false;
            }
        }

        private void btnTestProxy_Click(object sender, EventArgs e)
        {
            if (_appUpdate != null)
            {
                if (_appUpdate.IsGetUpdateInfoRunning)
                {
                    return;
                }
            }

            _appUpdate = new AppUpdater();
            //_appUpdate.Load += _appUpdate.Update_Load;
            _appUpdate.SetProxySettings(chkUseProxyForAutomaticUpdates.Checked, txtProxyAddress.Text,
                                        (int)numProxyPort.Value, chkUseProxyAuthentication.Checked,
                                        txtProxyUsername.Text,
                                        txtProxyPassword.Text);

            btnTestProxy.Enabled = false;
            btnTestProxy.Text = Language.OptionsProxyTesting;

            _appUpdate.GetUpdateInfoCompletedEvent += GetUpdateInfoCompleted;

            _appUpdate.GetUpdateInfoAsync();
        }

        private void chkUseProxyAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUseProxyForAutomaticUpdates.Checked)
            {
                pnlProxyAuthentication.Enabled = chkUseProxyAuthentication.Checked;
            }
        }

        #endregion

        private void GetUpdateInfoCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                AsyncCompletedEventHandler myDelegate = GetUpdateInfoCompleted;
                Invoke(myDelegate, sender, e);
                return;
            }

            try
            {
                _appUpdate.GetUpdateInfoCompletedEvent -= GetUpdateInfoCompleted;

                btnTestProxy.Enabled = true;
                btnTestProxy.Text = Language.ButtonTestProxy;

                if (e.Cancelled)
                {
                    return;
                }

                if (e.Error != null)
                {
                    throw e.Error;
                }

                CTaskDialog.ShowCommandBox(this, Application.ProductName, Language.ProxyTestSucceeded, "",
                                           Language.ButtonOK, false);
            }
            catch (Exception ex)
            {
                CTaskDialog.ShowCommandBox(this, Application.ProductName, Language.ProxyTestFailed,
                                           MiscTools.GetExceptionMessageRecursive(ex), "", "", "", Language.ButtonOK,
                                           false,
                                           ESysIcons.Error,
                                           0);
            }
        }

        #endregion
    }
}