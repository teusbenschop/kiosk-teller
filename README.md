# kiosk-teller

Start teller application in kiosk mode.

Run the KioskKassa.exe once, and leave it in the folder where you put it in.
Next time the KioskKassa.exe will start itself when the computer starts.
The KioskKassa.exe will start MplusQ.exe, a teller application.
It will keep MplusQ.exe full screen.

When a user starts Internet Explorer or Microsoft Edge, KioskKassa.exe will terminate them right away.

To stop KioskKassa.exe, do the following:
1. Close MplusQ.exe.
2. In KioskKassa.exe, enter a numerical code consisting of the current hour followed by the day of the month. Both numbers will have any leading zeroes. For example, on 6 March 13:00, the code to stop de app is: 1306
3. The KioskKassa.exe will terminate.

Some useful commands to run for support:

1. cmd /C taskkill /F /IM KioskKassa.exe
2. cmd /C taskkill /F /IM MplusQ.exe
