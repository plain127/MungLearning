from dotenv import load_dotenv
from langchain.chains import LLMChain
from langchain.chat_models import ChatOpenAI
from langchain.prompts import (
    ChatPromptTemplate,
    HumanMessagePromptTemplate,
    SystemMessagePromptTemplate,
)

load_dotenv()

class LlmModel():
    def __init__(self):
        self.llm_model = ChatOpenAI(model='gpt-4o', temperature=0)
        self.etc_cache = {}  # 캐시 저장소 분리
        self.health_cache = {}
        self.character_cache = {}
        self.color_cache = {}

        self.etc_prompt = ChatPromptTemplate.from_messages(
            [
                SystemMessagePromptTemplate.from_template('내용을 보고 칩 등록 여부, 보유물건, 성격, 건강을 각각 순서대로 마침표 단위로 분류해.'),
                HumanMessagePromptTemplate.from_template('{input}')
            ]
        )

        self.color_prompt = ChatPromptTemplate.from_messages(
            [
                SystemMessagePromptTemplate.from_template('''
                    입력 값은 유기견의 색이야. 
                    입력된 값이 흰색 계열의 색이면 흰색계열이라고 표시해.
                    입력된 값이 검정색 계열의 색이면 검정색계열이라고 표시해.
                    입력된 값이 황색 계열의 색이면 황색계열이라고 표시해.
                    입력된 값이 갈색 계열의 색이면 갈색계열이라고 표시해.
                    입력된 값이 혼합색 계열의 색이면 혼합색계열이라고 표시해.
                    입력된 값이 회색 계열의 색이면 회색계열이라고 표시해.

                    출력 :
                    흰색계열  
                    위의 형식처럼 하나의 계열만 출력해줘.
                '''), 
                HumanMessagePromptTemplate.from_template('{input}')
            ]
        )

        self.health_prompt = ChatPromptTemplate.from_messages(
            [
                SystemMessagePromptTemplate.from_template('''
                    입력 값은 유기견의 건강상태야. 
                    입력된 값의 상태에 건강이나 외모의 긍정적인 말이 작성되어 있다면 '건강'이라고 표시해.
                    입력된 값이 미상이면 '미상'이라고 표시해.
                    입력된 값에 질병이나 건강에 대한 부정적인 말이 작성되어 있다면 '질병' 이라고 표시해.
                    건강과 관련없는 외모의 특징인 경우에는 미상으로 처리해줘
                    출력 :
                    건강
                    위의 형식으로 출력해줘.
                '''), 
                HumanMessagePromptTemplate.from_template('{input}')
            ]
        )

        self.character_prompt = ChatPromptTemplate.from_messages(
            [
                SystemMessagePromptTemplate.from_template('''
                    입력 값은 유기견의 성격이야. 
                    입력된 값이 사람과 잘 어울리는 성격 계열이면 '사람을 잘 따름'이라고 표시해.
                    입력된 값이 사람과 잘 어울리지 못하는 성격 계열이면 '사람을 경계함'이라고 표시해.
                    입력된 값이 판별하기 어려운 성격 계열이면 '중립'이라고 표시해.
                    입력된 값이 없으면 '중립'이라고 표시해.

                    출력 :
                    사람을 잘 따름
                    위의 형식으로 출력해줘.
                '''), 
                HumanMessagePromptTemplate.from_template('{input}')
            ]
        )

        self.llm_chain = None
    
    def etc_cache_invoke(self, input_text):
        """기타 특징에 대한 캐시 처리"""
        if input_text in self.etc_cache:
            return self.etc_cache[input_text]
        result = self.llm_chain.invoke(input_text)['text']
        self.etc_cache[input_text] = result  # 캐시에 저장
        return result

    def health_cache_invoke(self, input_text):
        """건강 상태에 대한 캐시 처리"""
        if input_text in self.health_cache:
            return self.health_cache[input_text]
        result = self.llm_chain.invoke(input_text)['text']
        self.health_cache[input_text] = result  # 캐시에 저장
        return result

    def character_cache_invoke(self, input_text):
        """성격에 대한 캐시 처리"""
        if input_text in self.character_cache:
            return self.character_cache[input_text]
        result = self.llm_chain.invoke(input_text)['text']
        self.character_cache[input_text] = result  # 캐시에 저장
        return result

    def color_cache_invoke(self, input_text):
        """색상에 대한 캐시 처리"""
        if input_text in self.color_cache:
            return self.color_cache[input_text]
        result = self.llm_chain.invoke(input_text)['text']
        self.color_cache[input_text] = result  # 캐시에 저장
        return result
    
    def split_etc(self, df):
        self.llm_chain = LLMChain(
            llm=self.llm_model,
            prompt=self.etc_prompt
        )
        character = []
        health = []

        for etc in df[' 기타 특징']:
            result = self.etc_cache_invoke(etc)  # 캐시된 invoke 사용
            split_text = result.split('\n')
            character.append(next((s.split('성격: ')[1] for s in split_text if '성격: ' in s), None))
            health.append(next((s.split('건강: ')[1] for s in split_text if '건강: ' in s), None))
        
        df['성격'] = character
        df['건강'] = health
        df.drop([' 기타 특징'], axis=1, inplace=True)
        
    def health_preprocess(self, df):
        self.llm_chain = LLMChain(
            llm=self.llm_model,
            prompt=self.health_prompt
        )
        
        health = []
        
        for h in df['건강']:
            result = self.health_cache_invoke(h)  # 캐시된 invoke 사용
            health.append(result)
            
        df['건강'] = health
        
    def character_preprocess(self, df):
        self.llm_chain = LLMChain(
            llm=self.llm_model,
            prompt=self.character_prompt
        )
        
        character = []
        
        for h in df['성격']:
            result = self.character_cache_invoke(h)  # 캐시된 invoke 사용
            character.append(result)
            
        df['성격'] = character
    
    def color_preprocess(self, df):
        self.llm_chain = LLMChain(
            llm=self.llm_model,
            prompt=self.color_prompt
        )
        
        color = []
        
        for h in df['색']:
            result = self.color_cache_invoke(h)  # 캐시된 invoke 사용
            color.append(result)
            
        df['색'] = color
    # 디버그용 로그 출력 함수
    def log_cache_info(self):
        print("\n--- 캐시 상태 확인 ---")
        print(f"ETC 캐시 크기: {len(self.etc_cache)}")
        print(f"Health 캐시 크기: {len(self.health_cache)}")
        print(f"Character 캐시 크기: {len(self.character_cache)}")
        print(f"Color 캐시 크기: {len(self.color_cache)}")
        print("--- 끝 ---\n")