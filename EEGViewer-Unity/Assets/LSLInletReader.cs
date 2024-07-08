using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLInletReader : MonoBehaviour
{
    public float eegExpectedMean = 0.0f;
    public float eegExpectedVariance = 32768.0f;
    public float[] electrodeAdjust = new float[] { 
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                                            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };


    private string[] electrodes = new string[] {
        "AF7",
        "Fpz",
        "F7	",
        "Fz	",
        "T7	",
        "FC6",
        "Fp1",
        "F4	",
        "C4	",
        "Oz	",
        "CP6",
        "Cz	",
        "PO8",
        "CP5",
        "O2	",
        "O1	",
        "P3	",
        "P4	",
        "P7	",
        "P8	",
        "Pz	",
        "PO7",
        "T8	",
        "C3	",
        "Fp2",
        "F3	",
        "F8	",
        "FC5",
        "AF8"
        // A1
    };
    
    Electrode[] electrodeArray;

    // We need to find the stream somehow. You must provide a StreamType in editor or before this object is Started.
    public string StreamType = "EEG";
    ContinuousResolver resolver;
    public bool generateRandomEEGifNoStream = true;

    double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

    // We need to keep track of the inlet once it is resolved.
    private StreamInlet inlet;

    // We need buffers to pass to LSL when pulling data.
    private float[,] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
    private double[] timestamp_buffer;
    private int channel_count = -1;

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
        // example xml description of a stream
        // <settings><comport>1</comport><samplingrate>250</samplingrate><channelcount>32</channelcount></settings><channels>
        // <labels><label>AF7</label> ... <label>P1</label></labels></channels>

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
        // Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");
        channel_count = inlet.info().channel_count();
        data_buffer = new float[buf_samples, channel_count];
        timestamp_buffer = new double[buf_samples];

        ConnectElectrodes(inlet.info());
    }

    void ConnectElectrodes(StreamInfo info = null)
    {
        if (info != null)
        {
            // retrieve electrode names from stream info
            var xml = info.desc();

            int i = 0;
            var labels = xml.child("labels");
            var label = labels.first_child();
            while (!label.next_sibling().empty())
                electrodes[i++] = label.value();
        }

        // assign electrodes to electrodeArray by name
        electrodeArray = new Electrode[electrodes.Length];
        for (int i = 0; i < electrodes.Length; i++)
        {
            electrodeArray[i] = GameObject.Find(electrodes[i].Trim()).GetComponent<Electrode>();
            if (electrodeArray[i] == null)
                Debug.LogError("Could not find electrode: " + electrodes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inlet != null)
        {
            int samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);
            Debug.Log("Samples returned: " + samples_returned);
            //if (samples_returned > 0)
            for (int i = 0; i < electrodeArray.Length; i++)
            {
                if (i >= channel_count)
                    break;

                // for now just take the last sample for each eeg channel
                float eeg = data_buffer[samples_returned - 1, i];
                eeg = (eeg - eegExpectedMean) / eegExpectedVariance;
                eeg *= electrodeAdjust[i];
                // lerp color between red and greeen based on eeg value
                electrodeArray[i].fx.GetComponent<Light>().color = Color.Lerp(Color.red, Color.green, eeg/2.0f+0.5f);
            }
        }
        else if (generateRandomEEGifNoStream)
        {
            for (int i = 0; i < electrodeArray.Length; i++)
            {
                float eeg = Random.Range(-32768.0f, 32768.0f);
                eeg = (eeg - eegExpectedMean) / eegExpectedVariance;
                eeg *= electrodeAdjust[i];
                // lerp color between red and greeen based on eeg value
                electrodeArray[i].fx.GetComponent<Light>().color = Color.Lerp(Color.red, Color.green, eeg/2.0f+0.5f);
            }
        }
    }
}
