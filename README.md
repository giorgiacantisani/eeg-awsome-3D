# eeg-awesome-3D
Code for the eeg-awesome-3D project of Telluride 2024


## Setup
This project requires Git LFS (Large File Storage) extension. See installation guide: https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage


## Dependencies

To test with random data install:
* liblsl https://github.com/sccn/liblsl/releases
* PyLSL ```pip install pylsl``` 

Next, run command line: ```python -m pylsl.examples.SendDataAdvanced``` and press "play" (it looks like ">") button in Unity.

NOTE: currently test random data does not work on Mac, but only on Windows.
