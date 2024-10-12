# Unit Generator

Generates physics metric units, and performs compile time dimensional analysis

## Compile time dimensional analysis:
<img src="https://github.com/user-attachments/assets/084a0cf4-6a85-4167-b377-9c4821af0e0c" width="590">

Now you don't need to worry about messing up your physics equations

How it works:

<img src="https://github.com/user-attachments/assets/5dad8d69-4ea7-463a-bb92-7ca8c1881147" width="285">

you configure the generator and it generates all the units and operations with dimensional analysis already done.
```cs
public static N operator /(W power, mps speed) => new N(power.f / speed.f);
```

## Bonus feature - Units in your editor:

<img src="https://github.com/user-attachments/assets/49e1f656-aa80-41cd-8243-26036dfa0823" width="252">

Don't worry all the conversions happen in editor.

You can configure whatewer units you want in your editor.

Supports All the physics units: s, kg, m, A, K, mol, cd;

Bonus unit: rad for more efficien conversion from deg to rad in editor. 

## How to use: 
1. Create UnitGenerator ScriptableObject in a directory where you want to generate the units code.
    1. Alternatively you can Copy Packages/com.ovpavlov.unitgenerator/Editor/UnitGeneratorExample.asset for preconfigured generator example
2. Toggle 'Dry Run' and then press 'Generate Button'(which in fact is a Button that looks like a toggle), and check Console to see diagnostics. 
3. Untoggle 'Dry Run' and then press 'Generate Button' to generate the code 
