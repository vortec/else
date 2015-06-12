import traceback
try:
	raise ValueError("BLAH")
except ValueError as e:
	print traceback.format_exception(e)


