# Processes Priority Manager (PPM)
![image](https://github.com/user-attachments/assets/a706f4c9-9a3e-45a1-8a0c-05dc832b4817) ![image](https://github.com/user-attachments/assets/3574d2df-ded9-4758-83f5-81df23708c4e)


PPM is a tool that allows you to auto-apply two aspects related to process performance: priority in the system and processor affinity.

## Installation

You can use [GitHub releases](https://github.com/Taron-art/Processes-Priority-Manager/releases) to download the latest version of the tool.

## Usage
Because of the nature of changes the application is doing, the application requires administrator rights. 

You can start by adding a new profile (application name) to the list. To remove the process from the list, you need to set all settings to default, it will be removed on "Apply".
### Prioirty
You can select Cpu Priority (can be bigger or smaller than normal), IO and Memory priorities (can be only lower than normal). 

_Why this useful?_ 
* You can improve gaming (or productivity) performance by setting Cpu Priority to Above Normal;
* You can reduce some background process's (that you need to run) footprint (and battery usage) by lowering its priority. For example, you can set a torrent client to the lowest priority to avoid spikes when someone start downloading from you.

_How this works_ 

Initial priorities can be set using Window Registry and Windows will set them on process start, so this part is just a User Interface to change _HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\<Appname.exe>\PerfOptions_. Nothing is running in background to support this feature. This should never trigger any Anti-Cheat response since the game itself is never touched.

### Affinity

You can select CPU cores assigned to a specific application. 

_Why this useful?_
* You can improve gaming by allowing the game to run only on performance cores (on Intel) or only on one CCD (on AMD, for example on one with 3D V-Cache™).
* You can improve multitask performance by assigning different application with different cores. For example, Performance cores for Games, efficiency cores - On streaming.
* You can improve battery live by making some background processes to run only on efficient cores.

_How this works_

Whil you can set affinity manually to a running process using Task Manager, these are not saved for the next run. Also, there is no known API\Registry keys in Windows that allows to do that. To support this functionality, a tiny service was written. It monitors all new running processes and apply the selected Affinity when it finds a match. The footprint of the process is very small (usually, less than 1 MB of RAM), so it won't affect you system. This may (but should not) trigger an Anti-Cheat response since since the service is changing it on the fly.

### Limitations

Some applications (usually, some computer games) may control their priority and\or affinity. PPM was not designed to fight with that:
1. It usually means that authors of the application were trying to optimize this aspect and they probably know better. Priorities can be tweaked by the application itself, per thread to maximize performance.
1. This battle will probably be lost by PPM, only harming performance in process.

For example, DOOM Eternal will reset its priority to Normal if you try to change it, it also sets it is own affinity. Another example, Alan Wake 2 will reset its priority but not affinity.

## Performance testing

### Games

This list contains examples that were tested and actually found some performance improvements. Settings were set to the lowest possible to avoid GPU bottlenecking.

| Game				  | CPU Priority | Affinity | Average FPS | 1 % Low      | Notes									 |
|  ---				  |  ---         | ---      | ---         | ---          | ---									 |
| Counter-Strike 2    | Normal       | Default  | 614         | 200          | Game does not support affinity changes |
| Counter-Strike 2    | High         | Default  | 634 (+3.2%) | 207 (+3.5%)  | Settings are Very Low, 1920x1080		 |

More games will be added in the future.

### Applications

Test will be added in future.

## License

See the license during the installer. It is a MIT license with an exception related to Russian Federation.

## 🇺🇦 Please support Ukrainian Forces 🇺🇦

Please consider supporting Ukrainian Forces. You can donate to [Come Back Alive (Private Charity Foundation)](https://savelife.in.ua/en/donate-en/#donate-army-card-once), [Serhiy Prytula Charity Foundation (Private Charity Foundation)](https://prytulafoundation.org/en/donation) or [United24 (State Charity Foundation)](https://u24.gov.ua/).