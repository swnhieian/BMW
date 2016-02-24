#include <cstdio>
#include <iostream>
#include <string>
using namespace std;
int main() {
    string line;
    cin >> line;
    cin >> line;
    cin >> line;
    cin >> line;
    int num;
    cin >> num;
    for (int i=0; i<num; i++) {
        cin >> line;
        cin >> line;
        cin >> line;
        cout << line << endl;
    }
    return 0;
}