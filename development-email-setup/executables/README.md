# Development Email Setup

> https://mailtrap.io/blog/mailhog-explained/


There is one executable in `development-email-setup/executables` folder

- `MailHog_windows_amd64.exe`
	- This exe is for starting up the mail server locally
	- Visit http://localhost:8025 to view emails
	- When developing locally, this service starts up automatically
		- It will close if you kill the app with ctrl+c in the terminal
		- It won't close if you kill that app via VS
		- You can manually kill the service by going to task manager and searching for MailHog
