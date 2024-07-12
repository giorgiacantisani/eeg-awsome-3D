# eeg-awesome-3D
Code for the eeg-awesome-3D project of Telluride 2024

## Slide deck

https://docs.google.com/presentation/d/1xzQ1zwIQPY_zxPfgCSPdW2VmvVi7m97dMNGVeG4rU6c/edit#slide=id.p

## Setup
This project requires Git LFS (Large File Storage) extension. See installation guide: https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage


## Dependencies

To test with random data install:
* liblsl https://github.com/sccn/liblsl/releases
* PyLSL ```pip install pylsl``` 

Next, run command line: ```python -m pylsl.examples.SendDataAdvanced``` and press "play" (it looks like ">") button in Unity.

NOTE: currently test random data does not work on Mac, but only on Windows.




Brain models (Dean Lavery)
* https://sketchfab.com/3d-models/brain-areas-d64608a3978b47d8a39c5a15795ca8c4
* https://sketchfab.com/3d-models/brain-project-24ec03412dd8432bb0d3e750a72608e0
License: CC Attribution