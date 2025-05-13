# BobBoi Project - Copilot Instructions

This document provides guidance for GitHub Copilot users working on the BobBoi project.

## Project Context
We are developing a VR application for a Spatial User Interface class. The main component is a pipboy-like device called "BobBoi" that the user wears in VR and interacts with using various controls.

## Key Interactables

### 1. Year Selection Pull Handle
This is a handle that can be grabbed and pulled to select different years. When released, the user's avatar changes to reflect their appearance at that age. The handle should use XRGrabInteractable for interaction and track the pull distance to determine the selected year.

### 2. Screen Power Button
A simple button that toggles the device screen on and off. This should use a simple interactable to detect presses and toggle the visibility of the screen elements.

### 3. Hologram Brightness Thumbwheel
A rotatable wheel that controls the brightness of the holographic display. When rotated, it should adjust the brightness parameter of the hologram material.

## Coding Guidelines

1. **Keep it simple**
   - Write clear, straightforward code
   - Prefer readability over clever solutions
   - Use meaningful variable and function names

2. **Fail fast**
   - Avoid try/catch exceptions where possible
   - Use early returns and validation
   - Don't hide errors, make them obvious

3. **Inspector variables**
   - Avoid making variables editable in the inspector unless specified
   - For required inspector variables, use [SerializeField] with private access

4. **XR Interaction Patterns**
   - Use the XR Interaction Toolkit's built-in components
   - For grab interactions, extend from XRGrabInteractable
   - For simple button presses, use XRSimpleInteractable


When implementing new features, follow the existing pattern and keep the code modular and focused on a single responsibility.
