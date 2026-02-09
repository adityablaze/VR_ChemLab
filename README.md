# VR_ChemLab

**VR_ChemLab** is an interactive, educational Virtual Reality simulation designed to replicate a chemistry laboratory environment. This application allows users to perform experiments, handle apparatus, and observe chemical reactions in a safe, virtual space.

## 🎓 About The Project

This project was developed as a **Minor Project for the 5th Semester**.

It is important to note that this is primarily an **educational application** rather than a game. The goal was to create a modular system where students or anyone can interact with chemicals without the physical risks or costs associated with a real lab.

### ⏳ Development Timeline

This project was developed in a little bit of a short timeline. It took me about 1-2 months to complete the project alongside regular classes and other academic coursework. Even though the project was a little bit rushed, the core systems were designed to be modular and scalable. Because of this, I did make some use of AI to do things faster which otherwise would have taken longer for me to complete.

## ⚗️ Features

- **Interactive Apparatus:** Users can pick up and manipulate standard lab equipment including Beakers, Erlenmeyer Flasks, Test Tubes, and Tongs.
- **Pouring Mechanics:** A simple raycast based pouring system allowing transfer of fluids between containers.
- **Chemical Reactions:** A dynamic reaction system where mixing specific chemicals triggers visual changes (color change, particle effects) based on defined rules using scriptable objects.
- **VR Locomotion:** Implements standard VR movement (Teleportation and Continuous Move) and Snap/Smooth turning using the **Unity XR Interaction Toolkit**.
- **Educational Sandbox:** A safe environment to experiment with volatile substances.

## 🎥 Demo

_Watch a short video demonstration of the laboratory capabilities:_
[linkedInPost](https://www.linkedin.com/posts/aditya-nayak-77086727a_unity-virtualreality-xr-activity-7426747973633576961-Xd4A?utm_source=share&utm_medium=member_desktop&rcm=ACoAAEQdWYQBHJdKqeueaDZRsxaZkxwmqUUJDI8)
---

## 🛠️ Technical Details

### 1. Modular Chemical Rules (ScriptableObjects)

The core of the chemical interaction system relies on Unity's **ScriptableObjects**. Instead of hardcoding chemical properties into scripts, every chemical is an asset file.

- **Chemical Definitions:** Each chemical (e.g., H2O, HCl, NaOH, etc.) is a ScriptableObject defining properties like Name, Color, Density, and pH.
- **Reaction Rules:** Reactions are also defined as data. The system checks if `Chemical A` + `Chemical B` results in a valid `Reaction C`.

**Inspector Workflow:**
This modularity allows for a "Swap and Play" workflow. We can change the liquid inside a beaker simply by dragging a different Chemical ScriptableObject into the inspector slot before running the application. No code changes are required to introduce new chemicals.

![ezgif-89aa6a704860ef2c](https://github.com/user-attachments/assets/15ef1db5-510e-4ec2-9173-c49f429a4444)
![liq](https://github.com/user-attachments/assets/1690bb07-a151-4de3-aa61-e545c582ce1c)
![Wobble](https://github.com/user-attachments/assets/e203b88d-ccf9-418c-8a26-b7fb21213684)


### 2. Liquid Shader

To achieve a realistic look for the fluids, a custom shader was implemented. This shader handles:

- **Fill Level:** Adjusts the visual volume based on the container's fill amount.
- **Wobble/Slosh:** Reacts to the velocity and angular velocity of the container to simulate liquid movement.
- **Color Mixing:** Blends colors when two different chemicals are mixed based on the reaction rules.

![Shdergraph](https://github.com/user-attachments/assets/91e4fe7f-fa84-4924-a363-6ede1d7a0cb3)


### 3. XR Implementation

The project utilizes the **Unity XR Interaction Toolkit (3.2.1)**.
I have implemented both Telepotation and Contuious Move for locomotion, and Snap turning for rotation. I also implemented grabing and other simple interactions using the toolkit's built-in components.

## 📚 References & Credits

The development of the Liquid Shader and specific VR mechanics was heavily inspired by the following resources. Huge thanks to these creators for sharing their knowledge:

- **Shader Reference 1:** https://youtu.be/DKSpgFuKeb4
- **Shader Reference 2:** https://youtu.be/tI3USKIbnh0
- **Shader Reference 3:** https://youtu.be/p9NeBW4pgGk
- **Other Shader Reference:** https://www.patreon.com/posts/fake-liquid-urp-75665057

## 📦 Assets Used

- **3D Laboratory Environment:** Models for the interior props (shelves, tables) and apparatus (Beakers, Flasks, Bunsen Burners) were sourced from the "3D Laboratory Environment with Apparatus" pack.
- **XR Interaction Toolkit:** Unity Technologies.

---

_Developed by Aditya Nayak_
