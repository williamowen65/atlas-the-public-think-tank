# Development Email Setup

> https://mailtrap.io/blog/mailhog-explained/


There are two executables in `development-email-setup/executables` folder

- `MailHog_windows_amd64.exe`
	- This exe is for starting up the mail server locally
	- Visit http://localhost:8025 to view emails
- `mhsendmail_windows_amd64.exe`
	- This exe can be used to send mail to the test email server

```
@"
From: App <app@mailhog.local>
To: Test <test@mailhog.local>
Subject: Test message

Some content!
"@ | .\mhsendmail_windows_amd64.exe test@mailhog.local

```

```
@"
From: Atlas <noreply@thepublicthinktank.com>
To: William <william.owen.career@gmail.com>
Subject: HTML Test
Content-Type: text/html; charset=UTF-8

<html>
  <body>
    <h1 style="color:blue;">Hello from MailHog!</h1>
    <p>This is a <b>HTML email</b> test.</p>
  </body>
</html>
"@ | .\mhsendmail_windows_amd64.exe william.owen.career@gmail.com

```