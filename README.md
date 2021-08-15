# 偽装ちゃん

<p align="center">
  <image src="https://user-images.githubusercontent.com/22530106/119243859-9f354b00-bba5-11eb-8df7-c98c9721796e.png">
</p>
偽装ちゃん(FakeChan)は、

  棒読みちゃん(BouyomiChan)のフリをしてテキストを受け取りAssistantSeika経由で音声合成製品に渡します。 
コメントビュアーが棒読みちゃん呼び出しをサポートしていれば、棒読みちゃん替わりに利用できます。

…つまり、棒読みちゃんを停止して偽装ちゃんを実行すれば、棒読みちゃんから呼び出せなかった音声合成製品を呼び出せます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-114714.png "ダイアグラム")



## バイナリどこだよ？

私のサイトの[ダウンロードページ](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206)からダウンロードしてください。

## 使い方

事前に[AssistantSeika](https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/assistantseika-001a) をダウンロード、インストール、実行してください。

AssistantSeikaで利用したい音声合成製品を認識している状態になったら、偽装ちゃん(FakeChan.exe)を起動してください。  
コメントビュアーなどでは自動でBouyomiChan.exeを呼び出すようになっている場合があるので、
その際は呼び出すプログラムをFakeChan.exeに変更するか、FakeChan.exeをBouyomiChan.exeにリネームして入れ替えてください。  
※もちろんオリジナルのBouyomiChan.exeはバックアップしておいてください。

偽装ちゃんが起動すると、通信を受け付けてコメントを読み上げ可能になります。

### 状態表示タブ

偽装ちゃんの状態を表示する画面です。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-134108.png "偽装ちゃん起動直後")

  
棒読みちゃんを使うアプリケーションが棒読みちゃんと連携する時に使う受信インタフェースの有効化・無効化の状態と直近の受信データを表示しています。

偽装ちゃんと同じフォルダにファイル QuietMessages.json が存在すると、呟き機能が有効になります。
音声発声の待機状態が続くと指定の時間に指定の内容で呟き始めます。

### 話者設定タブ

各受信インタフェースで、棒読みちゃんの声質と音声合成製品を紐付けします。  

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-141812.png "話者マップ")

棒読みちゃん連携のソフトウエアのほとんどはボイス0のみ使っているように見受けられます。
ツイキャスコメントビューアー（閲覧君）は1番（女性1）を使用するようです。

発声方法には以下があります。
- 同期を選ぶと、メッセージはキューに入り、順次AssistantSeikaに発声指示を出します。
- 非同期を選ぶと、キューを使用せずすぐAssistantSeikaに発声指示を出します。

再生に使う音声合成製品は、同期と非同期で異なるものを指定してください。同じ音声合成製品を指定するのは無意味です。

### 音声設定タブ

音声合成製品のパラメタを編集できます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-142113.png "音声パラメタ編集")

テスト再生ボタンを押すと、再生テキストにあるテキストを設定したパラメタを適用して再生します。パラメタのテストで利用してください。

### 置換設定タブ

音声合成製品へ渡す前にテキスト情報を編集できます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-142307.png "置換設定")

正規表現による置換が一覧先頭から順に実施されます。適用する置換にはチェックを入れてください。  
最後に指定の文字数で切捨てが行われます。 

### アプリ設定タブ

アプリの共通的な設定を行います。
  
![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210702-142433.png "アプリ設定")

ウインドウタイトルを変更すると気に入らないアプリ名をとりあえず見なくて済むようにできます。

話者切り替え判定にチェックを入れると、日本語テキストではなさそうな場合、ここで指定した話者で発声するようになります。
ただし、この判定は大変適当なものです。
  
## その他の使い方

[偽装ちゃんと棒読みちゃんの混在方法](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206a)もどうぞ。


## 自分のアプリから連携

IPC, Socket, HTTP による接続のサンプルは棒読みちゃんに同梱されていますのでそちらをご覧ください。

## socket(50002)、HTTP(50081)の有効化

設定タブの丸をクリック。ON/OFF トグルスイッチになっています。

## 棒読みちゃん使えばいいんじゃないの？

64bit版の音声合成製品を利用できます……棒読みちゃんでCeVIO AI使うのがまだ困難なようですし… ※2021/03/16現在

