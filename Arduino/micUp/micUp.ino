const int micPin = A0; 
int maxVal = 0; 
int counter = 0;

void setup() {
  Serial.begin(9600);
  pinMode(micPin, INPUT);
}

void loop() {
  int micValue = analogRed(micPin);
  if (micValue>maxVal){
    maxVal = micVal
  }
  else {
    count ++;
    if(count == 10) {
      maxval = maxval-1;
      count = 0;
    }
  }

}
