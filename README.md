# Unit Generator

Generates physics metric units, and performs compile time dimensional analysis

## Compile time dimensional analysis:
![image](https://github.com/user-attachments/assets/084a0cf4-6a85-4167-b377-9c4821af0e0c)

Now you don't need to worry about messing up your physics equations

How it works:

![image](https://github.com/user-attachments/assets/5dad8d69-4ea7-463a-bb92-7ca8c1881147)

you configure the generator and it generates all the units and operations with dimensional analysis already done.
```cs
public static N operator /(W power, mps speed) => new N(power.f / speed.f);
```

## Bonus feature - Units in your editor:
![image](https://github.com/user-attachments/assets/49e1f656-aa80-41cd-8243-26036dfa0823)

Don't worry all the conversions happen in editor.
