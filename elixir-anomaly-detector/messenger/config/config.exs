use Mix.Config

config :messenger, Messenger.Mailer,
  adapter: Bamboo.SMTPAdapter,
  server: "localhost",
  hostname: "localhost",
  relay: "localhost",  # Replace with your SMTP relay server
  username: "your_username",  # Replace with your SMTP username
  password: "your_password",  # Replace with your SMTP password
  port: 1025,  # Replace with your SMTP port
  ssl: false,  # Set to true if your SMTP server uses SSL
  tls: false   # true  # Set to true if your SMTP server uses TLS


# config :messenger, Messenger.Mailer,
#   adapter: Bamboo.SMTPAdapter,
#   server: "localhost",
#   hostname: "localhost",
#   port: 1025,
#   username: "your.name@your.domain", # or {:system, "SMTP_USERNAME"}
#   password: "pa55word", # or {:system, "SMTP_PASSWORD"}
#   tls: :if_available, # can be `:always` or `:never`
#   allowed_tls_versions: [:"tlsv1", :"tlsv1.1", :"tlsv1.2"], # or {:system, "ALLOWED_TLS_VERSIONS"} w/ comma separated values (e.g. "tlsv1.1,tlsv1.2")
#   tls_log_level: :error,
#   tls_verify: :verify_peer, # optional, can be `:verify_peer` or `:verify_none`
#   tls_cacertfile: "/somewhere/on/disk", # optional, path to the ca truststore
#   tls_cacerts: "…", # optional, DER-encoded trusted certificates
#   tls_depth: 3, # optional, tls certificate chain depth
#   tls_verify_fun: {&:ssl_verify_hostname.verify_fun/3, check_hostname: "example.com"}, # optional, tls verification function
#   ssl: false, # can be `true`
#   retries: 1,
#   no_mx_lookups: false, # can be `true`
#   auth: :if_available # can be `:always`. If your smtp relay requires authentication set it to `:always`.
