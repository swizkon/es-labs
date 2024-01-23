defmodule Messenger.Email do
  # use Bamboo.Email, otp_app: :messenger
  import Bamboo.Email

  def welcome_email do
    new_email()
    |> from("your_email@example.com")
    |> to("recipient@example.com")
    |> subject("Welcome to MyApp!")
    |> text_body("Hello, welcome to MyApp!")
  end
end
