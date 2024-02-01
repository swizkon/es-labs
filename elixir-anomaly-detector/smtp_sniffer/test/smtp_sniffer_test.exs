defmodule SmtpSnifferTest do
  use ExUnit.Case
  doctest SmtpSniffer

  test "greets the world" do
    assert SmtpSniffer.hello() == :world
  end
end
