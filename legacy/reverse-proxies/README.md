The reverse proxy setup is a cost saving effort. 
It maps the custom domain to the Azure web app, and avoids the need
to upgrade the web app service just for use of a custom domain name.

It requires two files

- index.html file (simply required for a deployment)
- _redirects file with a splat
  - Note that there is no file extension.	


These are deployed with simple drag-n-drop deploys on Netlify.
Each deployment is it's own project on Netlify.

- The landing page app ( www.thepublicthinktank.com / atlas.thepublicthinktank.com )
	- This configures the DNS for the main domain
	- It is not a reverse-proxy.
	- When the app is ready to be published, this domain will be moved to Azure and the app service will be upgraded
- The Testing deployment ( testing.thepublicthinktank.com )
	- This deployment is meant for live testers of the app
	- It has it's own Netlify project for deployment.
- The Staging deployment ( staging.thepublicthinktank.com )
	- This deployment is meant for testing/preparing configuration of cloud resources / external APIs
	- It has it's own Netlify project for deployment.
