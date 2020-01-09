import itertools
import csv
  
r = 12
result = []
trialList = [[0 for x in range(5)] for y in range(20)]
answerList = [[0 for x in range(5)] for y in range(20)]

with open('trialCards.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    line_count = 0
    for row in csv_reader:
      for i in range(5):
        trialList[line_count][i] = row[i]
      line_count += 1
    
with open('answerCards.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    line_count = 0
    for row in csv_reader:
      for i in range(5):
        answerList[line_count][i] = row[i]
      line_count += 1

for aList in list(trialList):
  bList = answerList[trialList.index(aList)]
  matrix = [[0 for x in range(5)] for y in range(5)]
  for tmpA in list(aList):
    a = int(tmpA)
    for tmpB in list(bList):
      b = int(tmpB)
      matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a/r) - int(b/r)) + abs(a%r - b%r))
  result.append(min([sum([matrix[p[0]][p[1]] for p in enumerate(perm)]) for perm in itertools.permutations(range(len(matrix)))]))

resultString = ""
for a in list(result):
  resultString += str(a) + "\n"

f = open("result.csv", "w")
f.write(resultString)
f.close()