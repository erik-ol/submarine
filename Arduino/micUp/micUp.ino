const int micPin = A0; 
const int potPin = A3;
int micMaxVal = 0; 
int micCount = 0;

void setup() {
  Serial.begin(9600);
  pinMode(micPin, INPUT);
  pinMode(potPin, INPUT);
}

void loop() {
  int micValue = analogRead(micPin);
  if (micValue>micMaxVal){
    micMaxVal = micValue;
    Serial.print("Microphone: ");
    Serial.println(micMaxVal);
  }
  else {
    micCount ++;
    if(micCount == 10) {
      micMaxVal = micMaxVal-10;
      micCount = 0;
      Serial.print("Microphone: ");
      Serial.println(micMaxVal);
      
    }
  }

  int potValue = analogRead(potPin);
  Serial.print("Potentiometer: ");
  Serial.println(potValue);

}
