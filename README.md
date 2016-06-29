<a href="https://www.twilio.com">
  <img src="https://static0.twilio.com/marketing/bundles/marketing/img/logos/wordmark-red.svg" alt="Twilio" width="250" />
</a>

# Task Router with ASP.NET MVC

[![Build status](https://ci.appveyor.com/api/projects/status/73gd0opa4423fciu?svg=true)](https://ci.appveyor.com/project/TwilioDevEd/task-router-csharp)

Use Twilio to provide your user with multiple options through phone calls, so
they can be assisted by an agent specialized in the chosen topic. This is
basically a call center created with the Task Router API of Twilio. This example
uses a [SQLite](https://www.sqlite.org/) database to log phone calls which were
not assisted.

## Local Development

This project is built using [ASP.NET MVC](http://www.asp.net/mvc) Framework.

1. First clone this repository and `cd` into it.

   ```shell
   git clone git@github.com:TwilioDevEd/task-router-csharp.git
   cd task-router-csharp
   ```

1. Rename the sample configuration file and edit it to match your configuration.

   ```shell
   rename TaskRouter.Web\Local.config.example TaskRouter.Web\Local.config
   ```

   You can find your **Account SID** and **Auth Token** in your
   [Twilio Account](https://www.twilio.com/user/account/settings).

1. Build the solution.

1. Run the application.

1. Check it out at [http://localhost:8080](http://localhost:8080).

1. Expose application to the wider internet. To [start using
   ngrok](https://www.twilio.com/blog/2015/09/6-awesome-reasons-to-use-ngrok-when-testing-webhooks.html)
   on our project you'll have to execute the following line in the command
   prompt.

   ```shell
   ngrok http 8080 -host-header="localhost:8080"
   ```

## Meta

* No warranty expressed or implied. Software is as is. Diggity.
* [MIT License](http://www.opensource.org/licenses/mit-license.html)
* Lovingly crafted by Twilio Developer Education.
