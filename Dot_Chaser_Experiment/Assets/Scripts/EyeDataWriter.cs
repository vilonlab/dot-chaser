using System;
using System.IO;
using UnityEngine;

public class EyeDataWriter : MonoBehaviour
{
    private StreamWriter _writer;
    private ExperimentEyeGaze _eyeGaze;

    private void Awake()
    {
        Debug.Log("EyeDataWriter Awake");

        _eyeGaze = GetComponent<ExperimentEyeGaze>();
        if (_eyeGaze == null)
        {
            Debug.LogError("ExperimentEyeGaze component not found.");
            return;
        }
    
        _eyeGaze.EyeRotationUpdated += OnEyeRotationUpdated;
    
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var eyeId = _eyeGaze.Eye; // Get the EyeId
        var path = Path.Combine(PathConstants.EyeDataOutputPath, $"{timestamp}_EyeQuatPos_{eyeId}.csv");
        _writer = new StreamWriter(path);

        // Only write the header if the file is new
        if (new FileInfo(path).Length == 0)
        {
            _writer.WriteLine("Date,Time,Eye_Id," + // Write the header to the file
            "x_quaternion,y_quaternion,z_quaternion,w_quaternion," +
            "x_pos,y_pos,z_pos"); 
        }
    }

    private void OnEyeRotationUpdated(Quaternion eyeRotation, Vector3 eyePosition)
    {
        var eyeId = _eyeGaze.Eye; // Get the EyeId
        string date = DateTime.Now.ToString("yyyyMMdd");
        string time = DateTime.Now.ToString("HHmmss.fff");
        // Clean up the string representation of the Quaternion and Vector3
        string eyeRotationStr = eyeRotation.ToString().Replace("(", "").Replace(")", "");
        string eyePositionStr = eyePosition.ToString().Replace("(", "").Replace(")", "");

        // Write all the goodies to a file, line by line.
        _writer.WriteLine($"{date},{time},{eyeId},{eyeRotationStr},{eyePositionStr}"); // Write the EyeId and eyeRotation to the file
    }

    private void OnDestroy()
    {
        _eyeGaze.EyeRotationUpdated -= OnEyeRotationUpdated;
        _writer.Close();
    }
}