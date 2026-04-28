<div align="Center">
    <h3 align="Center">
      A VRChat OSC tool to maintain your eye height between avatar changes. <br>
    </h3>
    <div align="Center">
      <p align="Center">
        <a><img alt="Latest Version" src="https://img.shields.io/github/v/tag/ZenithVal/EyeHeightPersistence?color=informational&label=Version&sort=semver"></a>
        <a href="https://github.com/ZenithVal/EyeHeightPersistence/blob/main/LICENSE"><img alt="License" src="https://img.shields.io/github/license/ZenithVal/EyeHeightPersistence?label=Liscense"></a>
        <!-- <a><img alt="Downloads" src="https://img.shields.io/github/downloads/ZenithVal/EyeHeightPersistence/total.svg?label=Downloads"></a> --> 
        <!-- Enable this later zeni -->
        <br>
      </p>
    </div>
</div>

<!-- Why you looking at the raw readme, this is horrid to read. -->
<!-- If it's not obvious, I just took a majority of the readme from OSCLeash  lol. -->

<h1 align="Center">
  ⚠️ Currently only usable in VRChat's Open Beta! ⚠️<br>
</h1>

Keeps track of your avatar's height and reapplies it upon avatar changes. <br>
Optionally, it can scale proportionally to adjust for heels n such. <br>

# Download & Setup
Download the latest version of Eye Height Persistence [from releases](https://github.com/ZenithVal/EyeHeightPersistence/releases). <br>


# Config
After running the app at least once, a `config.json` file will be generated. <br>
You can open the json file in your favorite text editor and make edits.

---

| Value                 | Info                                                              | Default     |
|:--------------------- | ----------------------------------------------------------------- |:-----------:|
| IP                    | Address to send OSC data to                                       | 127.0.0.1   |
| ListeningPort         | Port to listen for OSC data on (if OSCQuery is false)             | 9001        |
| SendingPort           | Port to send OSC data to                                          | 9000        |
| OSCQuery              | Enables OSCQuery                                                  | true        |
| ChangeDelayMS         | Milliseconds to wait after an avatar change before applying       | 200         |
| RelativeMode          | Set eye height relative to base scale instead of 1:1              | true        |
| HeightTolerance       | Max difference in base heights before resorting to 1:1 eye height | 0.2         |
| ToleranceFailBehavior | Behavior when over tolerance. True = 1:1, False = Do nothing      | true        |


## Default config.json
```json
{
    "IP": "127.0.0.1",
    "ListeningPort": 9001,
    "SendingPort": 9000,
    "OSCQuery": true,
    "ChangeDelayMS": 200,
    "RelativeMode": true,
    "HeightTolerance": 0.2,
    "ToleranceFailBehavior": true
}
```


# Running from Source
- Clone the repo
- Open `EyeHeightPersistence.sln` in your IDE.
- Restore NuGet packages
- Build and Run

---