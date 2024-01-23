defmodule Messenger do
  @moduledoc """
  Documentation for `Messenger`.
  """

  @doc """
  Hello world.

  ## Examples

      iex> Messenger.send_welcome_email
      :world

  """

  def send_welcome_email do
    Messenger.Email.welcome_email()   # Create your email
    |> Messenger.Mailer.deliver_now!() # Send your email
  end

  @doc """
  Hello world.

  ## Examples

      iex> Messenger.hello()
      :world

  """
  def hello do
    IO.puts "Hello was called..."
    :world
  end
end
