﻿Windows usb hid string sample
\\?\hid#vid_046e&pid_52c3&col02#8&14b012ba&3&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}
Linux usb hid string sample
/sys/devices/sys/devices/pci0000:00/0000:00:1d.0/usb2/2-1/2-1.2/2-1.2:1.0/0003:046E:52C3.0002/hidraw/hidraw1

Preparation LINUX really work for hidraw devices
/etc/udev/rules.d/00-usb-permission.rules
KERNEL=="hidraw*", ATTRS{idVendor}=="04d8", ATTRS{idProduct}=="003f", TAG+="uaccess"

Once done, optionally rename this file for your application, and drop it into
/etc/udev/rules.d/00-usb-permission.rules

sudo udevadm control --reload-rules && sudo udevadm trigger