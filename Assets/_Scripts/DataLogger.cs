using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;

public class DataLogger {

    string filePath;
	private StreamWriter outputStream;

    public DataLogger(string fileName, bool forceOverride=true){
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath)){
            outputStream = File.CreateText(filePath);
        }
        else if (!forceOverride){ // quit out of the app instead of trying to override the existing participant data file
            Debug.Log("Trying to create a log file that already exists! Are you sure you want to override the file? The previously-logged data will be lost!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        Debug.Log("File ready at: " + filePath);
    }

    public void WriteLine(string data, bool newLine=true){
        if (outputStream != null) {
			if (newLine) {
				outputStream.WriteLine(data);
			} else {
				outputStream.Write(data);
			}
			outputStream.Flush();
		}
    }

    public void Close(){
        if (outputStream != null){
            outputStream.Close();
            outputStream.Dispose();
        }
    }

    public static string GetCurrentPath(){
        return UnityEngine.Application.dataPath;
    }

    public static string GetPersistentPath(){
        return UnityEngine.Application.persistentDataPath ;
    }

    public static bool FileExists(string fileName){
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        return File.Exists(fullPath);
    }

    // Properly implement IDisposable pattern
    public void Dispose(){
        Close();
    }
}
