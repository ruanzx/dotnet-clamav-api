#!/bin/bash

mkdir -p /var/log/clamav /var/run/clamav /var/log/supervisor
chown -R clamav:clamav /var/log/clamav /var/lib/clamav /var/run/clamav
chmod 750 /var/log/clamav /var/lib/clamav /var/run/clamav

# Start clamd service
service clamav-daemon start

# Add crontab entry for freshclam updates
# Set up hourly freshclam updates
echo "0 * * * * /usr/bin/freshclam --quiet" > /etc/cron.d/freshclam-cron \
  && chmod 0644 /etc/cron.d/freshclam-cron

# Start cron service
service cron start

# Update database for first time
freshclam

# Start supervisord
exec /usr/bin/supervisord -c /etc/supervisor/conf.d/supervisord.conf