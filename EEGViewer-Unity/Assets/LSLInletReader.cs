using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLInletReader : MonoBehaviour
{
    public bool movingAverage = true;
    public float movingAverageReactivity = 0.9f;

    public float EEGExpectedMean = 0.5f;
    public float EEGExpectedVariance = 0.25f;
    public float[] electrodeAdjust = new float[] { 
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };


    private string[] electrodes = new string[] {
        "AF7",
        "Fpz",
        "F7 ",
        "Fz ",
        "T7 ",
        "FC6",
        "Fp1",
        "F4 ",
        "C4 ",
        "Oz ",
        "CP6",
        "Cz ",
        "PO8",
        "CP5",
        "O2 ",
        "O1 ",
        "P3 ",
        "P4 ",
        "P7 ",
        "P8 ",
        "Pz ",
        "PO7",
        "T8 ",
        "C3 ",
        "Fp2",
        "F3 ",
        "F8 ",
        "FC5",
        "AF8"
        // A1
    };
    
    Electrode[] electrodeArray;

    // We need to find the stream somehow. You must provide a StreamType in editor or before this object is Started.
    public string StreamType = "EEG";
    ContinuousResolver resolver;
    public bool retrieveElectrodeNamesFromStream = false;
    public bool generateRandomEEGifNoStream = true;

    double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

    // We need to keep track of the inlet once it is resolved.
    private StreamInlet inlet;

    // We need buffers to pass to LSL when pulling data.
    float[,] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
    double[] timestamp_buffer;
    int channel_count = -1;
    float[] channels_min;
    float[] channels_max;

    public int eegChannelsToDisplay = 16;
    public float eegChannelOverlap = 0.25f;

    public ComputeShader eegDisplayCS;
    public RenderTexture eegDisplayRT;
    int eegDisplayIndex = 0;
    float[] channels_eeg = new float[32];
    float[] channels_eeg_prev = new float[32];
    
    void Start()
    {               
        Debug.Log("Protocol version: " + LSL.LSL.protocol_version());
        Debug.Log("Library version: " + LSL.LSL.library_version());

        ConnectElectrodes();

        StreamInfo[] streams = LSL.LSL.resolve_streams();
        Debug.Log(streams.Length);

        if (!StreamType.Equals(""))
            resolver = new ContinuousResolver("type", StreamType);
        else
            resolver = new ContinuousResolver();
        StartCoroutine(ResolveExpectedStream());
    }

    IEnumerator ResolveExpectedStream()
    {
        Debug.Log("ResolveExpectedStream");
        var results = resolver.results();
        while (results.Length == 0)
        {
            yield return new WaitForSeconds(.1f);
            Debug.Log("Not connected yet...");
            results = resolver.results();
        }

        inlet = new StreamInlet(results[0]);

        // Prepare pull_chunk buffer
        int buf_samples = (int)Mathf.Ceil((float)(inlet.info().nominal_srate() * max_chunk_duration));
        Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");
        channel_count = inlet.info().channel_count();
        Debug.Log("channels: " + channel_count);
        data_buffer = new float[buf_samples, channel_count];
        timestamp_buffer = new double[buf_samples];
        channels_max = new float[channel_count];
        channels_min = new float[channel_count];
        channels_eeg = new float[channel_count];
        for (var i = 0; i < channel_count; i++)
        {
            channels_max[i] = Mathf.NegativeInfinity;
            channels_min[i] = Mathf.Infinity;
            channels_eeg[i] = 0;
        }

        ConnectElectrodes(inlet.info());
    }

    void ConnectElectrodes(StreamInfo info = null)
    {
        if (retrieveElectrodeNamesFromStream && info != null)
        {
            // example xml description of a stream
            // <settings><comport>1</comport><samplingrate>250</samplingrate><channelcount>32</channelcount></settings><channels>
            // <labels><label>AF7</label> ... <label>P1</label></labels></channels>

            // retrieve electrode names from stream info
            var xml = info.desc();

            Debug.Log("XML from LSL stream: " + xml.value());

            int i = 0;
            var labels = xml.child("labels");
            var label = labels.first_child();
            while (!label.next_sibling().empty())
                electrodes[i++] = label.value();
        }

        Debug.Log("Electrodes: " + System.String.Join(" ", electrodes));

        // assign electrodes to electrodeArray by name
        electrodeArray = new Electrode[electrodes.Length];
        for (int i = 0; i < electrodes.Length; i++)
        {
            var go = GameObject.Find(electrodes[i].Trim());
            if (go == null)
            {
                Debug.LogWarning("Could not find electrode: " + electrodes[i]);
                continue;
            }

            electrodeArray[i] = go.GetComponent<Electrode>();
            if (electrodeArray[i] == null)
                Debug.LogError("Found object, but it has no Electrode component: " + electrodes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inlet != null)
        {
            int samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);
            // Debug.Log("Samples returned: " + samples_returned);
            // string eegs = "eegs: ";
            for (int j = 0; j < samples_returned; j++)
            {
                // double time = timestamp_buffer[j];

                for (int i = 0; i < electrodeArray.Length; i++)
                {
                    if (movingAverage)
                    {
                        var max = Mathf.Max(channels_max[i], data_buffer[j, i]);
                        var min = Mathf.Min(channels_min[i], data_buffer[j, i]);
                        if (channels_max[i] < -65535)
                            channels_max[i] = max;
                        else                  
                            channels_max[i] = Mathf.Lerp(channels_max[i], max, movingAverageReactivity);
                        if (channels_min[i] > 65535)
                            channels_min[i] = min;
                        else                  
                            channels_min[i] = Mathf.Lerp(channels_min[i], min, movingAverageReactivity);      
                    }
                    else
                    {
                        channels_max[i] = Mathf.Max(channels_max[i], data_buffer[j, i]);
                        channels_min[i] = Mathf.Min(channels_min[i], data_buffer[j, i]);
                    }
                }
            }

            if (samples_returned > 0)
            {
                for (int i = 0; i < electrodeArray.Length; i++)
                {
                    if (i >= channel_count)
                        break;

                    // for now just take the last sample for each eeg channel
                    float eeg = data_buffer[samples_returned - 1, i];

                    eeg = Mathf.InverseLerp(channels_min[i], channels_max[i], eeg);
                    eeg = (eeg - EEGExpectedMean) / EEGExpectedVariance;

                    // eegs += " " + eeg;
                    eeg *= electrodeAdjust[i];
                    // lerp color between red and greeen based on eeg value
                    if (electrodeArray[i])
                        electrodeArray[i].fx.GetComponent<Light>().color = Color.Lerp(Color.red, Color.green, eeg/2.0f+0.5f);

                    // vis
                    channels_eeg[i] = eeg/2+0.5f;
                }
            }
            //Debug.Log("Mooooo: " + eegs);
        }
        else if (generateRandomEEGifNoStream)
        {
            for (int i = 0; i < electrodeArray.Length; i++)
            {
                float eeg = Mathf.Sin(i + Time.time*i)/4 + 0.5f;// Random.Range(0.0f, 1.0f);
                // float eeg = Random.Range(0.0f, 1.0f)*EEGExpectedVariance+EEGExpectedMean;
                eeg = (eeg - EEGExpectedMean) / EEGExpectedVariance;
                eeg *= electrodeAdjust[i];
                // lerp color between red and greeen based on eeg value
                if (electrodeArray[i])
                    electrodeArray[i].fx.GetComponent<Light>().color = Color.Lerp(Color.red, Color.green, eeg/2.0f+0.5f);

                // vis
                channels_eeg[i] = eeg/2+0.5f;
            }
        }



        // Display EEG
        if (eegDisplayRT == null)
        {
            eegDisplayRT = new RenderTexture(1024, 256, 24);
            eegDisplayRT.enableRandomWrite = true;
            eegDisplayRT.Create();
        }
 
        eegDisplayCS.SetInt("WriteX", eegDisplayIndex);
        eegDisplayCS.SetInt("TextureHeight", eegDisplayRT.height);

        int kernelHandle = eegDisplayCS.FindKernel("CSPlotFade");
        eegDisplayCS.SetVector("Color", Color.black);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        kernelHandle = eegDisplayCS.FindKernel("CSPlotBar");
        eegDisplayCS.SetInt("WriteX", eegDisplayIndex+1);
        eegDisplayCS.SetVector("Color", Color.white);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        eegDisplayCS.SetInt("WriteX", eegDisplayIndex);
        eegDisplayCS.SetVector("Color", Color.black);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        kernelHandle = eegDisplayCS.FindKernel("CSPlotPixel");
        var amplitudeY = 30;
        var displayChannels = Mathf.Min(electrodeArray.Length, eegChannelsToDisplay);
        eegDisplayCS.SetInt("AmplitudeY", amplitudeY);
        eegDisplayCS.SetInt("OffsetY", (int)(256/displayChannels/(1.0f+eegChannelOverlap)));
        eegDisplayCS.SetInt("InputCount", displayChannels);
        eegDisplayCS.SetFloats("Inputs", channels_eeg);
        eegDisplayCS.SetFloats("InputsPrev", channels_eeg_prev);
        eegDisplayCS.SetVector("Color", Color.white);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, 1, amplitudeY, 1);

        eegDisplayIndex++; if (eegDisplayIndex >= eegDisplayRT.width) eegDisplayIndex = 0;
        for (int i = 0; i < electrodeArray.Length; i++)
            channels_eeg_prev[i] = channels_eeg[i];
    }
}
