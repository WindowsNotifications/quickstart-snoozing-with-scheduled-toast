# Quickstart: Enabling snoozeable notifications by using scheduled toast notifications and background tasks

Note that one caveat is that if the computer is off when the notification is supposed to re-appear, the notification will be dropped completely (unless the computer happens to be turned on within a 5 minute window of when the scheduled toast was supposed to be appear).

In other words, scheduled toast notifications have a 5 minute delivery window. If they miss that 5 minute delivery window (since the computer was off), the scheduled notification gets deleted and doesn't appear.