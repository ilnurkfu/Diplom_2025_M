from flask import Flask, request
from vosk import Model, KaldiRecognizer
import wave, json

model = Model("vosk-model-ru-0.42")  # Убедитесь, что модель скачана и находится в этой папке
app = Flask(__name__)

@app.route("/recognize", methods=["POST"])
def recognize():
    audio = request.files['file']
    wf = wave.open(audio, "rb")
    rec = KaldiRecognizer(model, wf.getframerate())

    result = ""
    while True:
        data = wf.readframes(4000)
        if len(data) == 0:
            break
        if rec.AcceptWaveform(data):
            res = json.loads(rec.Result())
            result += res.get("text", "") + " "
    res = json.loads(rec.FinalResult())
    result += res.get("text", "")
    
    return result.strip(), 200, {'Content-Type': 'text/plain'}

print("🚀 Vosk model loaded, starting Flask server on port 5005")
app.run(host="0.0.0.0", port=5005)
