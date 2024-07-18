using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLInletReader : MonoBehaviour
{
    public Gradient electrodeColors;
    public bool useColorGradient = false;

    public bool computeFourier = false;
    public float computeFourierFreqStep = 1.0f;
    public float computeFourierReactivity = 1.0f;


    public bool shortFourier = false;
    public float fourierReactivity = 0.1f;
    public float fourierMultiplier = 1.0f;

    public bool movingAverage = true;
    public float movingAverageReactivity = 0.9f;

    public float[] electrodeReA = new float[32];
    public float[] electrodeReB = new float[32];
    public float[] electrodeReC = new float[32];
    public float[] electrodeImA = new float[32];
    public float[] electrodeImB = new float[32];
    public float[] electrodeImC = new float[32];


    public float[] electrodeColorA = new float[32];
    public float[] electrodeColorB = new float[32];
    public float[] electrodeColorC = new float[32];

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

    private Vector3[] electrodePositions = new Vector3[] {
        /*1 	AF7 */  new Vector3( 68.7f, 49.7f,  -5.96f),
        /*2 	Fpz */  new Vector3(   85f,     0f, -1.79f),
        /*3 	F7  */  new Vector3( 49.9f,	 68.4f,	-7.49f),
        /*4 	Fz  */  new Vector3( 60.7f,	    0f,	 59.5f),
        /*5 	T7  */  new Vector3(    0f,  84.5f,	-8.85f),
        /*6 	FC6 */  new Vector3( 28.8f,	-76.2f,	 24.2f),
        /*7 	Fp1 */  new Vector3( 80.8f,	 26.1f,	 -4.0f),
        /*8 	F4  */  new Vector3( 57.6f, -48.1f,	 39.9f),
        /*9	    C4  */  new Vector3(    0f, -63.2f,	 56.9f),
        /*10	Oz  */  new Vector3(-85f,	    0f,	-1.79f),
        /*11	CP6 */  new Vector3(-28.8f,	-76.2f,	 24.2f),
        /*12	Cz  */  new Vector3(    0f,	    0f,    85f),
        /*13	PO8 */  new Vector3(-68.7f, -49.7f,	-5.95f),
        /*14	CP5 */  new Vector3(-28.8f,	 76.2f,	 24.2f),
        /*15	O2  */  new Vector3(-80.8f,	-26.1f,	   -4f),
        /*16	O1  */  new Vector3(-80.8f,	 26.1f,	   -4f),
        /*17	P3  */  new Vector3(-57.6f,	 48.2f,	 39.9f),
        /*18	P4  */  new Vector3(-57.6f,	-48.1f,	 39.9f),
        /*19	P7  */  new Vector3(-49.9f,	 68.4f,	-7.49f),
        /*20	P8  */  new Vector3(-49.9f,	-68.4f,	-7.49f),
        /*21	Pz  */  new Vector3(-60.7f,	    0f,	 59.5f),
        /*22	PO7 */  new Vector3(-68.7f,	 49.7f,	-5.96f),
        /*23	T8  */  new Vector3(    0f, -84.5f,	-8.85f),
        /*24	C3  */  new Vector3(    0f,  63.2f,	 56.9f),
        /*25	Fp2 */  new Vector3( 80.8f,	-26.1f,	   -4f),
        /*26	F3  */  new Vector3( 57.6f,	 48.2f,	 39.9f),
        /*27	F8  */  new Vector3( 49.9f,	-68.4f,	-7.49f),
        /*28	FC5 */  new Vector3( 28.8f,	 76.2f,	 24.2f),
        /*29	AF8 */  new Vector3( 68.7f,	-49.7f,	-5.95f),
        /*30	A1	*/  //new Vector3(    0f,  60.1f,	-60.1f),
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
    public bool eegDisplayInvert = false;

    public ComputeShader eegDisplayCS;
    public RenderTexture eegDisplayRT;
    public RenderTexture electrodeDisplayRT;
    public RenderTexture fftRT;
    int eegDisplayIndex = 0;
    int electrodeDisplayIndex = 0;    
    float[] channels_eeg = new float[32];
    float[] channels_eeg_prev = new float[32];
    Vector4[] electrode_colors = new Vector4[32*32];
    // float[] electrode_colors = new float[32*32*4];
    

    public int debugElectrode = -1;


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


        // clear EEG display
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = eegDisplayRT;
        GL.Clear(true, true, eegDisplayInvert ? Color.white : Color.black);
        RenderTexture.active = rt;
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
            System.Array.Resize(ref electrodes, i);
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

            var indicator = electrodeArray[i].indicator;
            if (indicator)
                foreach (var r in indicator.GetComponentsInChildren<Renderer>())
                {
                    var props = new MaterialPropertyBlock();
                    props.SetFloat("_Electrode", i);
                    r.SetPropertyBlock(props);
                    // var r = child.GetComponent<Renderer>();
                    // if (r)
                    // {
                    // }
                }
        }
    }

    Color MixEEGColors(float eeg)
    {
        if (useColorGradient)
            return electrodeColors.Evaluate(eeg/2.0f+0.5f);
        else
            return .4f*Color.Lerp(Color.red, Color.green, eeg/2.0f+0.5f);
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
                double time = timestamp_buffer[j];
                // Debug.Log(time);

                for (int i = 0; i < electrodeArray.Length; i++)
                {
                    if(i==0)
                    if (i >= channel_count)
                        break;

                    if(shortFourier)
                    {
                        float eeg = data_buffer[j,i];

                        float reComponent = eeg * Mathf.Cos((float)time * 10f * 2f * 3.14f);
                        float imComponent = eeg * Mathf.Sin((float)time * 10f * 2f * 3.14f);
                        // electrodeReA[i] =  Mathf.Lerp(electrodeReA[i], reComponent, fourierReactivity);
                        // electrodeImA[i] =  Mathf.Lerp(electrodeImA[i], imComponent, fourierReactivity);
                        electrodeReA[i] =  electrodeReA[i]*fourierReactivity + reComponent;
                        electrodeImA[i] =  electrodeImA[i]*fourierReactivity + imComponent;
                        electrodeColorA[i] = fourierMultiplier * Mathf.Sqrt(electrodeReA[i] * electrodeReA[i] + electrodeImA[i] * electrodeImA[i]);

                        float reComponent2 = eeg * Mathf.Cos((float)time * 20f * 2f * 3.14f);
                        float imComponent2 = eeg * Mathf.Sin((float)time * 20f * 2f * 3.14f);
                        // electrodeReB[i] =  Mathf.Lerp(electrodeReB[i], reComponent2, fourierReactivity);
                        // electrodeImB[i] =  Mathf.Lerp(electrodeImB[i], imComponent2, fourierReactivity);
                        electrodeReB[i] =  electrodeReB[i]*fourierReactivity + reComponent2;
                        electrodeImB[i] =  electrodeImB[i]*fourierReactivity + imComponent2;
                        electrodeColorB[i] = fourierMultiplier * Mathf.Sqrt(electrodeReB[i] * electrodeReB[i] + electrodeImB[i] * electrodeImB[i]);

                        float reComponent3 = eeg * Mathf.Cos((float)time * 40f * 2f * 3.14f);
                        float imComponent3 = eeg * Mathf.Sin((float)time * 40f * 2f * 3.14f);
                        // electrodeReC[i] =  Mathf.Lerp(electrodeReC[i], reComponent3, fourierReactivity);
                        // electrodeImC[i] =  Mathf.Lerp(electrodeImC[i], imComponent3, fourierReactivity);
                        electrodeReC[i] =  electrodeReC[i]*fourierReactivity + reComponent3;
                        electrodeImC[i] =  electrodeImC[i]*fourierReactivity + imComponent3;
                        electrodeColorC[i] = fourierMultiplier * Mathf.Sqrt(electrodeReC[i] * electrodeReC[i] + electrodeImC[i] * electrodeImC[i]);

                        // float reComponent = data_buffer[j,i] * Mathf.Cos((float)time * 10f * 2f * 3.14f);
                        // float imComponent = data_buffer[j,i] * Mathf.Sin((float)time * 10f * 2f * 3.14f);

                        // electrodeColorA[i] = Mathf.Lerp(electrodeColorA[i], Mathf.Sqrt(reComponent * reComponent + imComponent * imComponent), fourierReactivity);

                        // reComponent = data_buffer[j,i] * Mathf.Cos((float)time * 20f * 2f * 3.14f);
                        // imComponent = data_buffer[j,i] * Mathf.Sin((float)time * 20f * 2f * 3.14f);

                        // electrodeColorB[i] = Mathf.Lerp(electrodeColorB[i], Mathf.Sqrt(reComponent * reComponent + imComponent * imComponent), fourierReactivity);

                        // reComponent = data_buffer[j,i] * Mathf.Cos((float)time * 40f * 2f * 3.14f);
                        // imComponent = data_buffer[j,i] * Mathf.Sin((float)time * 40f * 2f * 3.14f);

                        // electrodeColorC[i] = Mathf.Lerp(electrodeColorC[i], Mathf.Sqrt(reComponent * reComponent + imComponent * imComponent), fourierReactivity);
                    }
                    else if (movingAverage)
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

                    if (debugElectrode >= 0 && i != debugElectrode)
                        eeg = 0;

                    // lerp color between red and greeen based on eeg value
                    if (electrodeArray[i])
                        electrodeArray[i].fx.GetComponent<Light>().color = MixEEGColors(eeg);

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
                float eeg = Mathf.Sin(i + Time.time*i)/4 + 0.5f;
                // float eeg = Mathf.Sin(Time.time * 2f * 2f * 3.14f);

                if(shortFourier)
                {
                    double time = Time.time;

                    float reComponent = eeg * Mathf.Cos((float)time * 10f * 2f * 3.14f);
                    float imComponent = eeg * Mathf.Sin((float)time * 10f * 2f * 3.14f);
                    // electrodeReA[i] =  Mathf.Lerp(electrodeReA[i], reComponent, fourierReactivity);
                    // electrodeImA[i] =  Mathf.Lerp(electrodeImA[i], imComponent, fourierReactivity);
                    electrodeReA[i] =  electrodeReA[i]*fourierReactivity + reComponent;
                    electrodeImA[i] =  electrodeImA[i]*fourierReactivity + imComponent;
                    electrodeColorA[i] = fourierMultiplier * Mathf.Sqrt(electrodeReA[i] * electrodeReA[i] + electrodeImA[i] * electrodeImA[i]);

                    float reComponent2 = eeg * Mathf.Cos((float)time * 20f * 2f * 3.14f);
                    float imComponent2 = eeg * Mathf.Sin((float)time * 20f * 2f * 3.14f);
                    // electrodeReB[i] =  Mathf.Lerp(electrodeReB[i], reComponent2, fourierReactivity);
                    // electrodeImB[i] =  Mathf.Lerp(electrodeImB[i], imComponent2, fourierReactivity);
                    electrodeReB[i] =  electrodeReB[i]*fourierReactivity + reComponent2;
                    electrodeImB[i] =  electrodeImB[i]*fourierReactivity + imComponent2;
                    electrodeColorB[i] = fourierMultiplier * Mathf.Sqrt(electrodeReB[i] * electrodeReB[i] + electrodeImB[i] * electrodeImB[i]);

                    float reComponent3 = eeg * Mathf.Cos((float)time * 40f * 2f * 3.14f);
                    float imComponent3 = eeg * Mathf.Sin((float)time * 40f * 2f * 3.14f);
                    // electrodeReC[i] =  Mathf.Lerp(electrodeReC[i], reComponent3, fourierReactivity);
                    // electrodeImC[i] =  Mathf.Lerp(electrodeImC[i], imComponent3, fourierReactivity);
                    electrodeReC[i] =  electrodeReC[i]*fourierReactivity + reComponent3;
                    electrodeImC[i] =  electrodeImC[i]*fourierReactivity + imComponent3;
                    electrodeColorC[i] = fourierMultiplier * Mathf.Sqrt(electrodeReC[i] * electrodeReC[i] + electrodeImC[i] * electrodeImC[i]);
                }

                eeg = (eeg - EEGExpectedMean) / EEGExpectedVariance;
                eeg *= electrodeAdjust[i];

                // eeg = Mathf.Sin(Time.time*2);
                // eeg = Random.Range(0.0f, 1.0f);

                if (debugElectrode >= 0 && i != debugElectrode)
                    eeg = 0;
                // lerp color between red and greeen based on eeg value
                if (electrodeArray[i])
                    electrodeArray[i].fx.GetComponent<Light>().color = MixEEGColors(eeg);

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

        eegDisplayCS.SetInt("Invert", eegDisplayInvert ? 1: 0); 
        eegDisplayCS.SetInt("WriteX", eegDisplayIndex);
        eegDisplayCS.SetInt("TextureHeight", eegDisplayRT.height);

        int kernelHandle = eegDisplayCS.FindKernel("CSPlotFade");
        eegDisplayCS.SetVector("Color", eegDisplayInvert ? Color.white : Color.black);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        kernelHandle = eegDisplayCS.FindKernel("CSPlotBar");
        eegDisplayCS.SetInt("WriteX", eegDisplayIndex+1);
        eegDisplayCS.SetVector("Color", eegDisplayInvert ? Color.black : Color.white);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        eegDisplayCS.SetInt("WriteX", eegDisplayIndex);
        eegDisplayCS.SetVector("Color", eegDisplayInvert ? Color.white : Color.black);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, eegDisplayRT.height/32, 1, 1);

        kernelHandle = eegDisplayCS.FindKernel("CSPlotPixel");
        var amplitudeY = 30;
        var displayChannels = Mathf.Min(electrodeArray.Length, eegChannelsToDisplay);
        if (channel_count > 0)
            displayChannels = Mathf.Min(displayChannels, channel_count);
        eegDisplayCS.SetInt("AmplitudeY", amplitudeY);
        eegDisplayCS.SetInt("OffsetY", (int)(256/displayChannels/(1.0f+eegChannelOverlap)));
        eegDisplayCS.SetInt("InputCount", displayChannels);
        eegDisplayCS.SetFloats("Inputs", channels_eeg);
        eegDisplayCS.SetFloats("InputsPrev", channels_eeg_prev);
        eegDisplayCS.SetVector("Color", eegDisplayInvert ? Color.black : Color.white);
        eegDisplayCS.SetTexture(kernelHandle, "Result", eegDisplayRT);
        eegDisplayCS.Dispatch(kernelHandle, 1, amplitudeY, 1);

        eegDisplayIndex++; if (eegDisplayIndex >= eegDisplayRT.width) eegDisplayIndex = 0;
        for (int i = 0; i < displayChannels; i++)
            channels_eeg_prev[i] = channels_eeg[i];

        
        // Display electrode indicators
        if (electrodeDisplayRT == null)
        {
            electrodeDisplayRT = new RenderTexture(32, 32, 24);
            electrodeDisplayRT.enableRandomWrite = true;
            electrodeDisplayRT.Create();
        }
        if (fftRT == null)
        {
            fftRT = new RenderTexture(32, 32, 24);
            fftRT.enableRandomWrite = true;
            fftRT.Create();
        }

        var indicatorChannels = electrodeArray.Length;
        if (channel_count > 0)
            indicatorChannels = Mathf.Min(indicatorChannels, channel_count);

        if (computeFourier)
        {
            for (int i = 0; i < indicatorChannels; i++)
                electrode_colors[i] = new Color(channels_eeg[i]*2f-1f, channels_eeg[i]*2f-1f, channels_eeg[i]*2f-1f, 0f);

            kernelHandle = eegDisplayCS.FindKernel("CSPlotPixelArray1D");
            eegDisplayCS.SetInt("WriteX", electrodeDisplayIndex);
            eegDisplayCS.SetVectorArray("Colors", electrode_colors);
            eegDisplayCS.SetInt("InputCount", indicatorChannels);
            eegDisplayCS.SetTexture(kernelHandle, "Result", fftRT);
            eegDisplayCS.Dispatch(kernelHandle, electrodeDisplayRT.height/32, 1, 1);

            kernelHandle = eegDisplayCS.FindKernel("CSFFT");
            eegDisplayCS.SetInt("WriteX", electrodeDisplayIndex);
            eegDisplayCS.SetFloat("Time", Time.time);
            eegDisplayCS.SetFloat("DeltaTime", Time.deltaTime);
            eegDisplayCS.SetFloat("FreqStep", computeFourierFreqStep);
            eegDisplayCS.SetFloat("ReactivityFFT", computeFourierReactivity);
            eegDisplayCS.SetTexture(kernelHandle, "Result", fftRT);
            eegDisplayCS.SetTexture(kernelHandle, "ResultFFT", electrodeDisplayRT);
            eegDisplayCS.Dispatch(kernelHandle, 1, 1, 1);

            electrodeDisplayIndex++; if (electrodeDisplayIndex >= electrodeDisplayRT.width) electrodeDisplayIndex = 0;

            // Shader.SetGlobalInt("_WriteX", electrodeDisplayIndex);
            Shader.SetGlobalInt("_WriteX", 0);

        }
        else if (!shortFourier)
        {
            for (int i = 0; i < indicatorChannels; i++)
                electrode_colors[i] = MixEEGColors(channels_eeg[i]*2f-1f);

            kernelHandle = eegDisplayCS.FindKernel("CSPlotPixelArray1D");
            eegDisplayCS.SetInt("WriteX", electrodeDisplayIndex);
            eegDisplayCS.SetVectorArray("Colors", electrode_colors);
            // eegDisplayCS.SetFloats("MyColors", electrode_colors);
            eegDisplayCS.SetInt("InputCount", indicatorChannels);
            eegDisplayCS.SetTexture(kernelHandle, "Result", electrodeDisplayRT);
            eegDisplayCS.Dispatch(kernelHandle, electrodeDisplayRT.height/32, 1, 1);

            electrodeDisplayIndex++; if (electrodeDisplayIndex >= electrodeDisplayRT.width) electrodeDisplayIndex = 0;

            Shader.SetGlobalInt("_WriteX", electrodeDisplayIndex);
        }
        else
        {
            for (int q = 0; q < 32; q++)
            {
                var fftBand = 
                    q<8 ? electrodeColorA :
                    q<16 ? electrodeColorB :
                    /*q == 2 ?*/ electrodeColorC;

                var color = 
                    q<8 ? electrodeColors.Evaluate(0f) :
                    q<16 ? electrodeColors.Evaluate(0.5f)  :
                    /*q == 2 ?*/ electrodeColors.Evaluate(1f) ;

                for (int i = 0; i < indicatorChannels; i++)
                    // electrode_colors[i] = MixEEGColors(fftBand[i]);
                    electrode_colors[i] = fftBand[i] * color;

                kernelHandle = eegDisplayCS.FindKernel("CSPlotPixelArray1D");
                eegDisplayCS.SetInt("WriteX", q);
                eegDisplayCS.SetVectorArray("Colors", electrode_colors);
                eegDisplayCS.SetInt("InputCount", indicatorChannels);
                eegDisplayCS.SetTexture(kernelHandle, "Result", electrodeDisplayRT);
                eegDisplayCS.Dispatch(kernelHandle, electrodeDisplayRT.height/32, 1, 1);

                Shader.SetGlobalInt("_WriteX", q);
            }

            Shader.SetGlobalInt("_WriteX", 0);
        }
    }
}
