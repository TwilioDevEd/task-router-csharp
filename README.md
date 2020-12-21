<a href="https://www.twilio.com">
  <img src="https://static0.twilio.com/marketing/bundles/marketing/img/logos/wordmark-red.svg" alt="Twilio" width="250" />
</a>

# Task Router with ASP.NET MVC

![](https://github.com/TwilioDevEd/task-router-csharp/workflows/NetFx/badge.svg)

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

1. Copy the sample configuration file and edit it to match your configuration.

   ```shell
   copy TaskRouter.Web\Local.config.example TaskRouter.Web\Local.config
   ```

   You can find your **Account SID** and **Auth Token** in your
   [Twilio Account](https://www.twilio.com/user/account/settings).

   Also part of the application initial config is the **HostUrl** it will be exposed to the wider internet.
   We will [use ngrok](https://www.twilio.com/blog/2015/09/6-awesome-reasons-to-use-ngrok-when-testing-webhooks.html)
   for exposing our local application. To start using ngrok on our project you'll have to
   execute the following line in the command prompt.

   ```shell
   ngrok http 8080 -host-header="localhost:8080"
   ```

   Now you can copy the **HostUrl**. It will be something like `https://26419c64.ngrok.io/`.

1. Build the solution.

1. Run the application.

1. Check it out at [http://localhost:8080](http://localhost:8080).

1. Configure Twilio to call your webhooks.

   You will also need to configure Twilio to call your application via POST when
   phone calls are received on your Twilio Number.

   The endpoint of **Voice** should look something like this:

   ```
   http://<sub-domain>.ngrok.io/call/incoming
   ```

   The endpoint of **SMS** should look something like this:

   ```
   http://<sub-domain>.ngrok.io/message/incoming
   ```

   ![Configure SMS](http://howtodocs.s3.amazonaws.com/twilio-number-config-all-med.gif)

## How To Demo?

1. Call your Twilio Phone Number. You will get a voice response:

   ```
   For Programmable SMS, press one.
   For Voice, press any other key.
   ```

1. Reply with 1.
1. The specified phone for the Programmable SMS agent's phone will be called.
1. If the Programmable SMS agent's phone is not answered in 30 seconds then the
   Programmable Voice agent's phone will be called.
1. In case the second agent doesn't answer the call, it will be logged as a
   missed call. You can see all missed calls in the main page of the running
   server at [http://localhost:8080](http://localhost:8080).
1. Repeat the process but enter any key different to __1__ to choose Voice.

[twilio-phone-number]: https://www.twilio.com/console/phone-numbers/incoming

## Meta

* No warranty expressed or implied. Software is as is. Diggity.
* [MIT License](http://www.opensource.org/licenses/mit-license.html)
* Lovingly crafted by Twilio Developer Education.
