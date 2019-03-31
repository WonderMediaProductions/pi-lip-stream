### Wonder Media Productions - internal setup steps

* VisualGDB notes
  - deploy to `/home/pi/Desktop/PiLipClientNative`
  - add the lib folder to the library path `export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/home/pi/Desktop/PiLipClientNative/lib`


* [Give each PI a unique network name](https://thepihut.com/blogs/raspberry-pi-tutorials/19668676-renaming-your-raspberry-pi-the-hostname)
  - `sudo nano /etc/hostname` => change name 
  -  `sudo nano /etc/hosts` => change name
  -  `hostname` => test name
  - I used `rp3` for my PI
  - This worked in the office, but not at home, I had to [install `sambda`](https://thisdavej.com/solution-for-cant-ping-raspberry-pi-hostname-on-the-network/) on the PI to make it discoverable. 
     - TODO: `samba` might also allow us to directly copy files to it without using SSH..
     - Editing the `etc/sambda/smb.conf` file and appending the lines below worked for me; however, `xcopy` was a lot slower than `rsync`

   ```
   [pi-desktop]
   path = /home/pi/Desktop/
   browsable = yes
   writable = yes
   read only = no
   force user = pi
   guest ok = yes
   security = user
   ```


* Define your PI's name in Windows 10 by [adding an environment variable](https://docs.telerik.com/teststudio/features/test-runners/add-path-environment-variables) `TARGET_PI=<YOUR_PI_NAME>`

* [Install .NET Core 2.2 on Windows 10](https://dotnet.microsoft.com/download/dotnet-core/2.2)

* [Install .NET Core 2.2 on Raspberry PI](https://www.hanselman.com/blog/InstallingTheNETCore2xSDKOnARaspberryPiAndBlinkingAnLEDWithSystemDeviceGpio.aspx)

* [Install remote debugging and deployment tools](https://github.com/Microsoft/MIEngine/wiki/Offroad-Debugging-of-.NET-Core-on-Linux---OSX-from-Visual-Studio)
   - On Windows, start `plink` once to register this private key
      - `CD` into the clone directory
      - Save your private key to `usr\private-key.ppk`
      - `"c:\Program Files\PuTTY\plink.exe" -i usr\private-key.ppk pi@%TARGET_PI%`
      - To synchronize files between the PI and Windows 10, I shared the `PiLipClient\bin` folder as `pi-lip-client`. 
         - I had to create a non-Microsoft-account local user (I called it `STS`) to get this working
      - On the PI:
         - `sudo apt-get install cifs-utils`
         - `sudo mkdir /mnt/pi-lip-client`
         - `sudo mount -t cifs //<WINDOWS_COMPUTER_NAME>/pi-lip-client /mnt/pi-lip-client -o username=STS,dir_mode=0777,file_mode=0777,serverino,sec=ntlmssp`  

*Note that I was unable to start the debugger with Visual Studio 2017 as described in the article, so for now we have to use VSCODE. If you find a solution, please notify me*

* [Install remote debugging and deployment tools for VSCode](https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx)

       
