using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace QuickstartSnoozingWithScheduledToast.Helpers
{
    public static class ToastHelper
    {
        public const string ACTION_REGISTER = "register";
        public const string ACTION_SNOOZE = "snooze";
        public const string INPUT_SNOOZEMINUTES = "snoozeMinutes";
        public const string TOAST_BACKGROUND_TASK = "ToastBackgroundTask";
        public const string TAG_REGISTER_REMINDER = "registerReminder";

        public static bool IsToastBackgroundTaskRegistered { get; private set; }

        public static XmlDocument GenerateToastReminderContent()
        {
            // Generate the toast content
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Be sure to register your new device!"
                            },
                            new AdaptiveText()
                            {
                                Text = "Click this notification to start the registration process."
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox(INPUT_SNOOZEMINUTES)
                        {
                            DefaultSelectionBoxItemId = "0.1",
                            Items =
                            {
                                new ToastSelectionBoxItem("0.1", "6 seconds"),
                                new ToastSelectionBoxItem("5", "5 minutes"),
                                new ToastSelectionBoxItem("15", "15 minutes"),
                                new ToastSelectionBoxItem("1440", "1 day"),
                                new ToastSelectionBoxItem("4320", "3 days")
                            }
                        }
                    },
                    Buttons =
                    {
                        // Snoozing will activate our background task
                        new ToastButton("Snooze", ACTION_SNOOZE)
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        // Dismissing will delete the notification
                        new ToastButtonDismiss()
                    }
                },
                Launch = ACTION_REGISTER,
                Scenario = ToastScenario.Reminder
            };

            return toastContent.GetXml();
        }

        public static async Task RegisterBackgroundTaskAsync()
        {
            try
            {
                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(TOAST_BACKGROUND_TASK)))
                {
                    IsToastBackgroundTaskRegistered = true;
                    return;
                }

                // Otherwise request access
                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

                // Create the background task
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
                {
                    Name = TOAST_BACKGROUND_TASK
                };

                // Assign the toast action trigger
                builder.SetTrigger(new ToastNotificationActionTrigger());

                // And register the task
                BackgroundTaskRegistration registration = builder.Register();

                // Flag that the task is registered
                IsToastBackgroundTaskRegistered = true;
            }
            catch
            {
                // Report to telemetry
            }
        }

        public static void HandleBackgroundActivation(ToastNotificationActionTriggerDetail details)
        {
            try
            {
                switch (details.Argument)
                {
                    case ACTION_SNOOZE:
                        HandleBackgroundSnoozeActivation(details);
                        break;
                }
            }
            catch
            {
                // Report to telemetry
            }
        }

        private static void HandleBackgroundSnoozeActivation(ToastNotificationActionTriggerDetail details)
        {
            string snoozeMinutesTxt = (string)details.UserInput[INPUT_SNOOZEMINUTES];
            double snoozeMinutes = double.Parse(snoozeMinutesTxt);

            // Generate the reminder toast content once again
            XmlDocument toastContentDoc = GenerateToastReminderContent();

            // Calculate the time we want the notification to re-appear based on the snooze time the user selected
            DateTimeOffset deliveryTime = DateTime.Now.AddMinutes(snoozeMinutes);

            // Create a scheduled notification with these values
            ScheduledToastNotification scheduledNotif = new ScheduledToastNotification(toastContentDoc, deliveryTime)
            {
                Tag = TAG_REGISTER_REMINDER
            };

            // And schedule the notification
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
        }
    }
}
