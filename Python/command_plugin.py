from Else import Command

# example of query from c#
demoquery = {
    'Raw': "testkw 12345",
    'Arguments': " 12345",
    'Empty': False,
    'HasArguments': True,
    'Keyword': "testkw",
    'KeywordComplete': "true"
}

test = Command(keyword='testkw', title='TITLE', subtitle='SUBTITLE')
print(test.is_interested(demoquery))
print(test.query(demoquery, None))